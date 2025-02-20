using Kingmaker.Blueprints;
using Kingmaker.Utility;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.BinaryFormat;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.Modding;
using System.Collections.Concurrent;
using UnityEngine;
using System.Diagnostics;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints;
public class BlueprintLoader {
    private List<SimpleBlueprint?> m_BlueprintBeingLoaded = null!;
    private List<SimpleBlueprint>? m_Blueprints;
    private Dictionary<Type, List<SimpleBlueprint>> m_BlueprintsByType = new();
    private HashSet<SimpleBlueprint> m_BlueprintsToAdd = new();
    private HashSet<BlueprintGuid> m_BlueprintsToRemove = new();
    public bool CanStart = false;
    public static BlueprintLoader BPLoader { get; } = new();
    public readonly HashSet<string> BadBlueprints = ["ce0842546b73aa34b8fcf40a970ede68", "2e3280bf21ec832418f51bee5136ec7a", "b60252a8ae028ba498340199f48ead67", "fb379e61500421143b52c739823b4082", "5d2b9742ce82457a9ae7209dce770071"];
    public BlueprintLoader() {
        var toPatch = AccessTools.Method(typeof(StartGameLoader), nameof(StartGameLoader.LoadPackTOC));
        var patch = AccessTools.Method(typeof(BlueprintLoader), nameof(InitPatch));
        Main.HarmonyInstance.Patch(toPatch, finalizer: new(patch));

        toPatch = AccessTools.Method(typeof(BlueprintsCache), nameof(BlueprintsCache.AddCachedBlueprint));
        patch = AccessTools.Method(typeof(BlueprintLoader), nameof(AddCachedBlueprintPatch));
        Main.HarmonyInstance.Patch(toPatch, postfix: new(patch));

        toPatch = AccessTools.Method(typeof(BlueprintsCache), nameof(BlueprintsCache.RemoveCachedBlueprint));
        patch = AccessTools.Method(typeof(BlueprintLoader), nameof(RemoveCachedBlueprintPatch));
        Main.HarmonyInstance.Patch(toPatch, prefix: new(patch));

        toPatch = AccessTools.Method(typeof(BlueprintsCache), nameof(BlueprintsCache.Load));
        patch = AccessTools.Method(typeof(BlueprintLoader), nameof(BlueprintsCache_LoadPrefix));
        var patch2 = AccessTools.Method(typeof(BlueprintLoader), nameof(BlueprintsCache_LoadPostfix));
        Main.HarmonyInstance.Patch(toPatch, prefix: new(patch), postfix: new(patch2));
    }
    public List<SimpleBlueprint>? GetBlueprints() {
        if (m_Blueprints == null) {
            lock (this) {
                if (IsLoading) {
                    return null;
                } else {
                    Debug($"Starting Blueprint Loading");
                    Load((bps) => {
                        lock (m_BlueprintsToAdd) {
                            bps.AddRange(m_BlueprintsToAdd);
                            m_BlueprintsToAdd.Clear();
                        }
                        lock (m_BlueprintsToRemove) {
                            bps.RemoveAll(bp => m_BlueprintsToRemove.Contains(bp.AssetGuid));
                            m_BlueprintsToRemove.Clear();
                        }
                        m_Blueprints = bps;
                        if (BlueprintIdCache.NeedsCacheRebuilt) BlueprintIdCache.RebuildCache(m_Blueprints);
                        m_BlueprintsByType.Clear();
                    });
                    return null;
                }
            }
        } else {
            if (m_BlueprintsToAdd.Count > 0) {
                lock (m_BlueprintsToAdd) {
                    m_Blueprints.AddRange(m_BlueprintsToAdd);
                    m_BlueprintsToAdd.Clear();
                }
            }
            if (m_BlueprintsToRemove.Count > 0) {
                lock (m_BlueprintsToRemove) {
                    m_Blueprints.RemoveAll(bp => m_BlueprintsToRemove.Contains(bp.AssetGuid));
                    m_BlueprintsToRemove.Clear();
                }
            }
            return m_Blueprints;
        }
    }
    public static IEnumerable<SimpleBlueprint>? BlueprintsOfType(Type type) {
        return (IEnumerable<SimpleBlueprint>)AccessTools.Method(typeof(BlueprintLoader), nameof(GetBlueprintsOfType)).MakeGenericMethod(type).Invoke(BPLoader, null);
    }
    public IEnumerable<BPType>? GetBlueprintsOfType<BPType>() where BPType : SimpleBlueprint {
        if (m_Blueprints == null) {
            if (Settings.UseBPIdCache && !BlueprintIdCache.NeedsCacheRebuilt) {
                if (m_BlueprintsByType.TryGetValue(typeof(BPType), out var bps)) {
                    return bps.Cast<BPType>();
                } else if (BlueprintIdCache.Instance.IdsByType.TryGetValue(typeof(BPType), out var ids)) {
                    Load(bps => {
                        lock (m_BlueprintsToAdd) {
                            IEnumerable<BPType> toAdd = m_BlueprintsToAdd.OfType<BPType>();
                            if (toAdd != null) {
                                m_BlueprintsToAdd.RemoveRange(toAdd);
                                bps.AddRange(toAdd);
                            }
                        }
                        // Is this really necessary?
                        lock (m_BlueprintsToRemove) {
                            if (m_BlueprintsToRemove.Count > 0) {
                                bps.ToArray().ForEach(bp => {
                                    if (m_BlueprintsToRemove.Contains(bp.AssetGuid)) {
                                        m_BlueprintsToRemove.Remove(bp.AssetGuid);
                                        bps.Remove(bp);
                                    }
                                });
                            }
                        }
                        m_BlueprintsByType[typeof(BPType)] = bps;
                    }, ids);
                    return null;
                }
            }
            return GetBlueprints()?.OfType<BPType>();
        } else {
            return m_Blueprints.OfType<BPType>();
        }
    }
    public IEnumerable<BPType> GetBlueprintsByGuids<BPType>(IEnumerable<string> guids) where BPType : SimpleBlueprint {
        foreach (var guid in guids) {
            var bp = ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(guid)) as BPType;
            if (bp != null) {
                yield return bp;
            }
        }
    }
    private int m_TotalLoading;
    private int m_EstimateLoaded;
    private Action<List<SimpleBlueprint>> m_OnFinishLoading = null!;
    private List<ConcurrentDictionary<BlueprintGuid, object>> m_StartedLoadingShards = new();
    private List<Task> m_WorkerTasks = new();
    private ConcurrentQueue<IEnumerable<(BlueprintGuid, int)>> m_ChunkQueue = null!;
    private void Load(Action<List<SimpleBlueprint>> callback, ISet<BlueprintGuid>? toLoad = null) {
        if (IsLoading
            || (!CanStart && Game.Instance.Player == null)
            || m_Blueprints != null) {
            return;
        }

        m_EstimateLoaded = 0;
        m_StartedLoadingShards.Clear();
        for (int i = 0; i < Settings.BlueprintsLoaderNumShards; i++) {
            m_StartedLoadingShards.Add(new());
        }
        m_OnFinishLoading = callback;
        m_WorkerTasks.Clear();
        IsLoading = true;
        Task.Run(() => Run(toLoad));
    }
    public float Progress {
        // This will probably throw if multiple it's called more than once per event
        get {
            if (Event.current.type == EventType.Layout) {
                if (m_TotalLoading > 0) {
                    field = (float)m_EstimateLoaded / m_TotalLoading;
                } else {
                    field = 0;
                }
            }
            return field;
        }
    }
    public bool IsLoading = false;
    public bool HasLoaded => m_Blueprints != null;
    public void Run(ISet<BlueprintGuid>? toLoad) {
        var watch = Stopwatch.StartNew();
        var bpCache = ResourcesLibrary.BlueprintsCache;
        IEnumerable<BlueprintGuid> allEntries;
        var toc = bpCache.m_LoadedBlueprints;
        if (toLoad == null) {
            allEntries = toc.OrderBy(e => e.Value.Offset).Select(e => e.Key);
        } else {
            allEntries = toc.Where(item => toLoad.Contains(item.Key)).OrderBy(e => e.Value.Offset).Select(e => e.Key);
        }
        m_TotalLoading = allEntries.Count();
        Log($"Loading {m_TotalLoading} Blueprints");
        m_BlueprintBeingLoaded = new(m_TotalLoading);
        m_BlueprintBeingLoaded.AddRange(Enumerable.Repeat<SimpleBlueprint?>(null, m_TotalLoading));
        var memStream = new MemoryStream();
        lock (bpCache.m_Lock) {
            bpCache.m_PackFile.Position = 0;
            bpCache.m_PackFile.CopyTo(memStream);
        }
        var chunks = allEntries.Select((entry, index) => (entry, index)).Chunk(Settings.BlueprintsLoaderChunkSize);
        m_ChunkQueue = new(chunks);
        var bytes = memStream.GetBuffer();
        for (int i = 0; i < Settings.BlueprintsLoaderNumThreads; i++) {
            var t = Task.Run(() => HandleChunks(bytes));
            m_WorkerTasks.Add(t);
        }
        foreach (var task in m_WorkerTasks) {
            task.Wait();
        }
        m_BlueprintBeingLoaded.RemoveAll(b => b is null);
        watch.Stop();
        Log($"Threaded loaded {m_BlueprintBeingLoaded.Count + m_BlueprintsToAdd.Count + m_LoadingSequentially.Count} blueprints in {watch.ElapsedMilliseconds} milliseconds");
        toLoad = null;
        lock (this) {
            m_OnFinishLoading(m_BlueprintBeingLoaded!);
            IsLoading = false;
        }
    }
    // External mods could register their own actions here
    public Action<BlueprintGuid> OnAfterBPLoad = _ => { };
    public Action<BlueprintGuid> OnBeforeBPLoad = _ => { };
    public void HandleChunks(byte[] bytes) {
        try {
            Stream stream = new MemoryStream(bytes);
            stream.Position = 0;
            var seralizer = new ReflectionBasedSerializer(new PrimitiveSerializer(new BinaryReader(stream), UnityObjectConverter.AssetList));
            int closeCountLocal = 0;
            while (m_ChunkQueue.TryDequeue(out var blueprintChunk)) {
                if (closeCountLocal > 100) {
                    lock (m_BlueprintBeingLoaded) {
                        m_EstimateLoaded += closeCountLocal;
                    }
                    closeCountLocal = 0;
                }
                foreach ((BlueprintGuid bpToLoad, int index) bpEntry in blueprintChunk) {
                    var guid = bpEntry.bpToLoad;
                    try {
                        object @lock = new();
                        lock (@lock) {
                            int shardIndex = Math.Abs(guid.GetHashCode()) % Settings.BlueprintsLoaderNumShards;
                            var startedLoading = m_StartedLoadingShards[shardIndex];
                            if (!startedLoading.TryAdd(guid, @lock)) continue;
                            if (ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.TryGetValue(guid, out var entry)) {
                                if (entry.Blueprint != null) {
                                    closeCountLocal++;
                                    m_BlueprintBeingLoaded[bpEntry.index] = entry.Blueprint;
                                    continue;
                                }
                            } else {
                                continue;
                            }
                            if (BadBlueprints.Contains(guid.ToString()) || entry.Offset == 0U) continue;
                            OnBeforeBPLoad(guid);
                            stream.Seek(entry.Offset, SeekOrigin.Begin);
                            SimpleBlueprint? simpleBlueprint = null;
                            seralizer.Blueprint(ref simpleBlueprint);
                            if (simpleBlueprint == null) {
                                closeCountLocal++;
                                continue;
                            }
                            object obj;
                            OwlcatModificationsManager.Instance.OnResourceLoaded(simpleBlueprint, guid.ToString(), out obj);
                            simpleBlueprint = (obj as SimpleBlueprint) ?? simpleBlueprint;
                            entry.Blueprint = simpleBlueprint;
                            simpleBlueprint.OnEnable();
                            m_BlueprintBeingLoaded[bpEntry.index] = simpleBlueprint;
                            ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints[guid] = entry;
                            closeCountLocal++;
                            OnAfterBPLoad(guid);
                        }
                    } catch (Exception ex) {
                        Warn($"Exception loading blueprint {guid}:\n{ex}");
                        closeCountLocal++;
                    }
                }
            }
        } catch (Exception ex) {
            Error($"Exception loading blueprints:\n{ex}");
        }
    }
    private static void AddCachedBlueprintPatch(BlueprintGuid guid, SimpleBlueprint bp) {
        if (BPLoader.IsLoading || BPLoader.m_Blueprints != null) {
            if (BPLoader.IsLoading) {
                int shardIndex = Math.Abs(guid.GetHashCode()) % Settings.BlueprintsLoaderNumShards;
                BPLoader.m_StartedLoadingShards[shardIndex].TryAdd(guid, BPLoader);
            }
            lock (BPLoader.m_BlueprintsToAdd) {
                BPLoader.m_BlueprintsToAdd.Add(bp);
            }
        }
    }
    private static void RemoveCachedBlueprintPatch(BlueprintGuid guid) {
        lock (BPLoader.m_BlueprintsToRemove) {
            BPLoader.m_BlueprintsToRemove.Add(guid);
        }
    }
    private static HashSet<BlueprintGuid> m_LoadingSequentially = new();
    public static bool BlueprintsCache_LoadPrefix(BlueprintGuid guid, ref SimpleBlueprint __result) {
        // If threaded loader is not activated just load normally
        if (!BPLoader.IsLoading) return true;
        int shardIndex = Math.Abs(guid.GetHashCode()) % Settings.BlueprintsLoaderNumShards;
        var startedLoading = BPLoader.m_StartedLoadingShards[shardIndex];
        if (startedLoading.TryAdd(guid, BPLoader)) {
            // If the requested bp was not yet touched by the threaded loader, just load normally
            m_LoadingSequentially.Add(guid);
            return true;
        }
        // The requested bp was touched by the threaded loader, so lock on the object to wait for the loading to complete
        lock (startedLoading[guid]) {
            if (ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.TryGetValue(guid, out var entry)) {
                __result = entry.Blueprint;
            } else {
                __result = null!;
            }
        }
        return false;
    }
    public static void BlueprintsCache_LoadPostfix(BlueprintGuid guid, ref SimpleBlueprint __result) {
        if (m_LoadingSequentially.Contains(guid)) {
            m_LoadingSequentially.Remove(guid);
            lock (BPLoader.m_BlueprintsToAdd) {
                if (__result != null) BPLoader.m_BlueprintsToAdd.Add(__result);
            }
        }
    }
    private static void InitPatch() {
        BPLoader.CanStart = true;
        if (Settings.PreloadBlueprints || (Settings.UseBPIdCache && Settings.AutomaticallyBuildBPIdCache && BlueprintIdCache.NeedsCacheRebuilt)) BPLoader.GetBlueprints();
    }
}
