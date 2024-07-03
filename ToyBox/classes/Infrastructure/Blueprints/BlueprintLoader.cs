// Copyright < 2021 > Narria(github user Cabarius) - License: MIT
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.BinaryFormat;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.Modding;
using ModKit;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ToyBox {
    public class BlueprintLoader {
        public delegate void LoadBlueprintsCallback(List<SimpleBlueprint> blueprints);
        private List<SimpleBlueprint> blueprints;
        private HashSet<SimpleBlueprint> bpsToAdd = new();
        internal bool CanStart = false;
        public float progress = 0;
        private static BlueprintLoader loader;
        public static BlueprintLoader Shared {
            get {
                loader ??= new();
                return loader;
            }
        }
        internal readonly HashSet<string> BadBlueprints = new() { "ce0842546b73aa34b8fcf40a970ede68", "2e3280bf21ec832418f51bee5136ec7a",
            "b60252a8ae028ba498340199f48ead67", "fb379e61500421143b52c739823b4082", "5d2b9742ce82457a9ae7209dce770071" };
        private void Load(LoadBlueprintsCallback callback) {
            lock (loader) {
                if (IsLoading || (!CanStart && Game.Instance.Player == null)) return;
                loader.Init(callback);
            }
        }
        public bool IsLoading => loader.IsRunning;
        public List<SimpleBlueprint> GetBlueprints() {
            if (blueprints == null) {
                lock (loader) {
                    if (Shared.IsLoading) { return null; } else {
                        Mod.Debug($"calling BlueprintLoader.Load");
                        Shared.Load((bps) => {
                            bps.AddRange(bpsToAdd);
                            bpsToAdd.Clear();
                            blueprints = bps;
                        });
                        return null;
                    }
                }
            }
            if (bpsToAdd.Count > 0) {
                blueprints.AddRange(bpsToAdd);
                bpsToAdd.Clear();
            }
            return blueprints;
        }
        public List<BPType> GetBlueprints<BPType>() {
            var bps = GetBlueprints();
            return bps?.OfType<BPType>().ToList() ?? null;
        }
        internal IEnumerable<BPType> GetBlueprintsByGuids<BPType>(IEnumerable<BlueprintGuid> guids) where BPType : BlueprintFact {
            var bps = GetBlueprints<BPType>();
            return bps?.Where(bp => guids.Contains(bp.AssetGuid));
        }
        public IEnumerable<BPType> GetBlueprintsByGuids<BPType>(IEnumerable<string> guids) where BPType : BlueprintFact => GetBlueprintsByGuids<BPType>(guids.Select(BlueprintGuid.Parse));
        [HarmonyPatch]
        internal static class BlueprintLoaderPatches {
            [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.AddCachedBlueprint)), HarmonyPostfix]
            internal static void AddCachedBlueprint(BlueprintGuid guid, SimpleBlueprint bp) {
                if (Shared.IsLoading || Shared.blueprints != null) {
                    Shared.bpsToAdd.Add(bp);
                }
            }
            [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.RemoveCachedBlueprint)), HarmonyPostfix]
            internal static void RemoveCachedBlueprint(BlueprintGuid guid) {
                Shared.bpsToAdd.RemoveWhere(bp => bp.AssetGuid == guid);
            }
            [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Start)), HarmonyPostfix]
            internal static void PreparePregensCoroutine() {
                Shared.CanStart = true;
                if (Main.Settings.PreloadBlueprints) Shared.GetBlueprints();
            }
        }
        public bool IsRunning = false;
        private LoadBlueprintsCallback _callback;
        private List<Task> _workerTasks;
        private ConcurrentQueue<IEnumerable<(KeyValuePair<BlueprintGuid, BlueprintsCache.BlueprintCacheEntry>, int)>> _chunkQueue;
        private List<SimpleBlueprint> _blueprints;
        private int closeCount;
        private int total;
        public void Init(LoadBlueprintsCallback callback) {
            IsRunning = true;
            _callback = callback;
            Task.Run(Run);
        }
        public void Run() {
            var watch = Stopwatch.StartNew();
            var bpCache = ResourcesLibrary.BlueprintsCache;
            var toc = bpCache.m_LoadedBlueprints;
            var allEntries = toc.OrderBy(e => e.Value.Offset);
            total = allEntries.Count();
            Mod.Log($"Loading {total} Blueprints");
            closeCount = 0;
            _blueprints = new(total);
            _blueprints.AddRange(Enumerable.Repeat<SimpleBlueprint>(null, total));
            var memStream = new MemoryStream();
            bpCache.m_PackFile.Position = 0;
            bpCache.m_PackFile.CopyTo(memStream);
            var chunks = allEntries.Select((entry, index) => (entry, index)).Chunk(Main.Settings.BlueprintsLoaderChunkSize);
            _chunkQueue = new(chunks);
            var bytes = memStream.GetBuffer();
            _workerTasks = new();
            for (int i = 0; i < Main.Settings.BlueprintsLoaderNumThreads; i++) {
                var t = Task.Run(() => HandleChunks(bytes));
                _workerTasks.Add(t);
            }
            Task.Run(Progressor);
            foreach (var task in _workerTasks) {
                task.Wait();
            }
            _blueprints.RemoveAll(b => b is null);
            watch.Stop();
            Mod.Log($"Threaded loaded {_blueprints.Count} blueprints in {watch.ElapsedMilliseconds} milliseconds");
            lock (loader) {
                IsRunning = false;
                _callback(_blueprints);
            }
        }
        public void HandleChunks(byte[] bytes) {
            try {
                Stream stream = new MemoryStream(bytes);
                stream.Position = 0;
                var seralizer = new ReflectionBasedSerializer(new PrimitiveSerializer(new BinaryReader(stream), UnityObjectConverter.AssetList));
                int closeCountLocal = 0;
                while (_chunkQueue.TryDequeue(out var entries)) {
                    if (closeCountLocal > 100) {
                        lock (_blueprints) {
                            closeCount += closeCountLocal;
                        }
                        closeCountLocal = 0;
                    }
                    foreach (var entryPairA in entries) {
                        var entryPair = entryPairA.Item1;
                        try {
                            var entry = entryPair.Value;
                            if (entry.Blueprint != null) {
                                closeCountLocal += 1;
                                _blueprints[entryPairA.Item2] = entry.Blueprint;
                            }
                            if (Shared.BadBlueprints.Contains(entryPair.Key.ToString()) || entry.Offset == 0U) {
                                closeCountLocal++;
                                continue;
                            }
                            OnBeforeBPLoad(entryPair.Key);
                            stream.Position = entry.Offset;
                            SimpleBlueprint simpleBlueprint = null;
                            seralizer.Blueprint(ref simpleBlueprint);
                            if (simpleBlueprint == null) {
                                closeCountLocal++;
                                continue;
                            }
                            object obj;
                            OwlcatModificationsManager.Instance.OnResourceLoaded(simpleBlueprint, entryPair.Key.ToString(), out obj);
                            simpleBlueprint = (obj as SimpleBlueprint) ?? simpleBlueprint;
                            simpleBlueprint.OnEnable();
                            _blueprints[entryPairA.Item2] = simpleBlueprint;
                            entry.Blueprint = simpleBlueprint;
                            ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints[entryPair.Key] = entry;
                            closeCountLocal++;
                            OnAfterBPLoad(entryPair.Key);
                        } catch (Exception ex) {
                            Mod.Log($"Exception loading blueprint {entryPair.Key}:\n{ex}");
                            closeCountLocal++;
                        }
                    }
                }
            } catch (Exception ex) {
                Mod.Log($"Exception loading blueprints:\n{ex}");
            }
        }
        // These methods exist to allow external mods some interfacing since the bp load bypasses the regular BlueprintsCache.Load.
        // Not using delegate since those would have problems with reloading during runtime.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OnBeforeBPLoad(BlueprintGuid bp) {

        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OnAfterBPLoad(BlueprintGuid bp) {

        }
        public void Progressor() {
            while (loader.IsRunning) {
                progress = closeCount / (float)total;
                progress = progress < 0 ? 0 : progress > 1 ? 1 : progress;
                Thread.Sleep(200);
            }
        }
    }
    public static class BlueprintLoader<BPType> {
        public static IEnumerable<BPType> blueprints = null;
    }
}
