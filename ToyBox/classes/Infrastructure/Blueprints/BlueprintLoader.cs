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
                            lock (bpsToAdd) {
                                bps.AddRange(bpsToAdd);
                                bpsToAdd.Clear();
                            }
                            blueprints = bps;
                        });
                        return null;
                    }
                }
            }
            lock (bpsToAdd) {
                if (bpsToAdd.Count > 0) {
                    blueprints.AddRange(bpsToAdd);
                    bpsToAdd.Clear();
                }
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
                    lock (Shared.bpsToAdd) {
                        Shared.bpsToAdd.Add(bp);
                    }
                }
                if (Shared.IsRunning) Shared._startedLoading.TryAdd(guid, Shared);
            }
            [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.RemoveCachedBlueprint)), HarmonyPostfix]
            internal static void RemoveCachedBlueprint(BlueprintGuid guid) {
                lock (Shared.bpsToAdd) {
                    Shared.bpsToAdd.RemoveWhere(bp => bp.AssetGuid == guid);
                }
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
        private ConcurrentQueue<IEnumerable<(BlueprintGuid, int)>> _chunkQueue;
        private List<SimpleBlueprint> _blueprints;
        private ConcurrentDictionary<BlueprintGuid, Object> _startedLoading = new();
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
            var allEntries = toc.OrderBy(e => e.Value.Offset).Select(e => e.Key);
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
            Mod.Log($"Threaded loaded {_blueprints.Count + bpsToAdd.Count} blueprints in {watch.ElapsedMilliseconds} milliseconds");
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
                        var guid = entryPairA.Item1;
                        try {
                            Object @lock = new();
                            lock (@lock) {
                                if (!_startedLoading.TryAdd(guid, @lock)) continue;
                                if (ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.TryGetValue(guid, out var entry) && entry.Blueprint != null) {
                                    closeCountLocal += 1;
                                    _blueprints[entryPairA.Item2] = entry.Blueprint;
                                }
                                if (Shared.BadBlueprints.Contains(guid.ToString()) || entry.Offset == 0U) {
                                    closeCountLocal++;
                                    continue;
                                }
                                OnBeforeBPLoad(guid);
                                stream.Seek(entry.Offset, SeekOrigin.Begin);
                                SimpleBlueprint simpleBlueprint = null;
                                seralizer.Blueprint(ref simpleBlueprint);
                                if (simpleBlueprint == null) {
                                    closeCountLocal++;
                                    continue;
                                }
                                object obj;
                                OwlcatModificationsManager.Instance.OnResourceLoaded(simpleBlueprint, guid.ToString(), out obj);
                                simpleBlueprint = (obj as SimpleBlueprint) ?? simpleBlueprint;
                                simpleBlueprint.OnEnable();
                                _blueprints[entryPairA.Item2] = simpleBlueprint;
                                entry.Blueprint = simpleBlueprint;
                                ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints[simpleBlueprint.AssetGuid] = entry;
                                closeCountLocal++;
                                OnAfterBPLoad(guid);
                            }
                        } catch (Exception ex) {
                            Mod.Log($"Exception loading blueprint {guid}:\n{ex}");
                            closeCountLocal++;
                        }
                    }
                }
            } catch (Exception ex) {
                Mod.Log($"Exception loading blueprints:\n{ex}");
            }
        }
        [HarmonyPatch(typeof(BlueprintsCache))]
        public static class LoaderPatches {
            private static bool IsLoading = false;
            [HarmonyPatch(nameof(BlueprintsCache.Load)), HarmonyPrefix]
            public static bool Pre_Load(BlueprintGuid guid, ref SimpleBlueprint __result) {
                if (!Shared.IsRunning) return true;
                if (Shared._startedLoading.TryAdd(guid, Shared)) {
                    IsLoading = true;
                    return true;
                } else {
                    lock (Shared._startedLoading[guid]) {
                        __result = ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints[guid].Blueprint;
                    }
                    return false;
                }
            }
            [HarmonyPatch(nameof(BlueprintsCache.Load)), HarmonyPostfix]
            public static void Post_Load(BlueprintGuid guid, ref SimpleBlueprint __result) {
                if (IsLoading) {
                    IsLoading = false;
                    lock (Shared.bpsToAdd) {
                        Shared.bpsToAdd.Add(__result);
                    }
                }
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
}
