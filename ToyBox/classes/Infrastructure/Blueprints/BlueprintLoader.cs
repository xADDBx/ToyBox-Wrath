// Copyright < 2021 > Narria(github user Cabarius) - License: MIT
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.BinaryFormat;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.DLC;
using Kingmaker.Modding;
using Kingmaker.Utility;
using ModKit;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ToyBox.classes.Infrastructure.Blueprints;

namespace ToyBox {
    public class BlueprintLoader {
        public static string GameVersion;
        public delegate void LoadBlueprintsCallback(List<SimpleBlueprint> blueprints);
        private List<SimpleBlueprint> blueprints;
        private Dictionary<Type, List<SimpleBlueprint>> bpsByType = new();
        private HashSet<SimpleBlueprint> bpsToAdd = new();
        internal bool CanStart = false;
        public float progress = 0;
        private static BlueprintLoader loader;
        public static BlueprintLoader Shared {
            get {
                if (GameVersion.IsNullOrEmpty()) {
                    GameVersion = Kingmaker.GameVersion.GetVersion();
                }
                loader ??= new();
                return loader;
            }
        }
        internal readonly HashSet<string> BadBlueprints = new() { "ce0842546b73aa34b8fcf40a970ede68", "2e3280bf21ec832418f51bee5136ec7a",
            "b60252a8ae028ba498340199f48ead67", "fb379e61500421143b52c739823b4082", "5d2b9742ce82457a9ae7209dce770071" };
        private void Load(LoadBlueprintsCallback callback, ISet<BlueprintGuid> toLoad = null) {
            lock (loader) {
                if (IsLoading || (!CanStart && Game.Instance.Player == null)) return;
                loader.Init(callback, toLoad);
            }
        }
        public bool IsLoading => loader.IsRunning;
        public List<SimpleBlueprint> GetBlueprints() {
            if (blueprints == null) {
                lock (loader) {
                    if (Shared.IsLoading) {
                        return null;
                    } else {
                        Mod.Debug($"calling BlueprintLoader.Load");
                        Shared.Load((bps) => {
                            lock (bpsToAdd) {
                                bps.AddRange(bpsToAdd);
                                bpsToAdd.Clear();
                            }
                            blueprints = bps;
                            if (BlueprintIdCache.NeedsCacheRebuilt) BlueprintIdCache.RebuildCache(blueprints);
                            bpsByType.Clear();
                        }, toLoad);
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
        public static IEnumerable<SimpleBlueprint> BlueprintsOfType(Type type) {
            return (IEnumerable<SimpleBlueprint>)AccessTools.Method(typeof(BlueprintLoader), nameof(GetBlueprintsOfType)).MakeGenericMethod(type).Invoke(Shared, null);
        }
        public IEnumerable<BPType> GetBlueprintsOfType<BPType>() where BPType : SimpleBlueprint {
            if (blueprints == null) {
                if (Main.Settings.toggleUseBPIdCache && !BlueprintIdCache.NeedsCacheRebuilt) {
                    if (bpsByType.TryGetValue(typeof(BPType), out var bps)) {
                        return bps.Cast<BPType>();
                    } else if (BlueprintIdCache.Instance.IdsByType.TryGetValue(typeof(BPType), out var ids)) {
                        if (!Shared.IsLoading) {
                            Load(bps => {
                                IEnumerable<BPType> toAdd;
                                toAdd = bpsToAdd.OfType<BPType>() ?? new List<BPType>();
                                bps.AddRange(toAdd);
                                bpsByType[typeof(BPType)] = bps;
                            }, ids);
                        }
                        return new List<BPType>();
                    }
                }
            } else {
                return blueprints.OfType<BPType>();
            }
            return GetBlueprints()?.OfType<BPType>() ?? new List<BPType>();
        }
        public IEnumerable<BPType> GetBlueprintsByGuids<BPType>(IEnumerable<string> guids) where BPType : SimpleBlueprint {
            foreach (var guid in guids) {
                var bp = ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(guid)) as BPType;
                if (bp != null) {
                    yield return bp;
                }
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
        private ISet<BlueprintGuid> toLoad;
        public void Init(LoadBlueprintsCallback callback, ISet<BlueprintGuid> toLoad) {
            closeCount = 0;
            _startedLoading.Clear();
            _callback = callback;
            _workerTasks = new();
            this.toLoad = toLoad;
            IsRunning = true;
            Task.Run(Run);
        }
        public void Run() {
            var watch = Stopwatch.StartNew();
            var bpCache = ResourcesLibrary.BlueprintsCache;
            IEnumerable<BlueprintGuid> allEntries;
            var toc = bpCache.m_LoadedBlueprints;
            if (toLoad == null) {
                allEntries = toc.OrderBy(e => e.Value.Offset).Select(e => e.Key);
            } else {
                allEntries = toc.Where(item => toLoad.Contains(item.Key)).OrderBy(e => e.Value.Offset).Select(e => e.Key);
            }
            total = allEntries.Count();
            Mod.Log($"Loading {total} Blueprints");
            _blueprints = new(total);
            _blueprints.AddRange(Enumerable.Repeat<SimpleBlueprint>(null, total));
            var memStream = new MemoryStream();
            lock (bpCache.m_Lock) {
                bpCache.m_PackFile.Position = 0;
                bpCache.m_PackFile.CopyTo(memStream);
            }
            var chunks = allEntries.Select((entry, index) => (entry, index)).Chunk(Main.Settings.BlueprintsLoaderChunkSize);
            _chunkQueue = new(chunks);
            var bytes = memStream.GetBuffer();
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
            Mod.Log($"Threaded loaded {_blueprints.Count + bpsToAdd.Count + BlueprintLoaderPatches.BlueprintsCache_Patches.IsLoading.Count} blueprints in {watch.ElapsedMilliseconds} milliseconds");
            toLoad = null;
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
                                if (ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.TryGetValue(guid, out var entry)) {
                                    if (entry.Blueprint != null) {
                                        closeCountLocal++;
                                        _blueprints[entryPairA.Item2] = entry.Blueprint;
                                        continue;
                                    }
                                } else {
                                    continue;
                                }
                                if (Shared.BadBlueprints.Contains(guid.ToString()) || entry.Offset == 0U) continue;
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
                                entry.Blueprint = simpleBlueprint;
                                simpleBlueprint.OnEnable();
                                _blueprints[entryPairA.Item2] = simpleBlueprint;
                                ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints[guid] = entry;
                                closeCountLocal++;
                                OnAfterBPLoad(guid);
                            }
                        } catch (Exception ex) {
                            Mod.Warn($"Exception loading blueprint {guid}:\n{ex}");
                            closeCountLocal++;
                        }
                    }
                }
            } catch (Exception ex) {
                Mod.Error($"Exception loading blueprints:\n{ex}");
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
        [HarmonyPatch]
        internal static class BlueprintLoaderPatches {
            [HarmonyPatch(typeof(BlueprintsCache))]
            internal static class BlueprintsCache_Patches {
                [HarmonyPatch(nameof(BlueprintsCache.AddCachedBlueprint)), HarmonyPostfix]
                internal static void AddCachedBlueprint(BlueprintGuid guid, SimpleBlueprint bp) {
                    if (Shared.IsLoading || Shared.blueprints != null) {
                        lock (Shared.bpsToAdd) {
                            Shared.bpsToAdd.Add(bp);
                        }
                    }
                    if (Shared.IsRunning) Shared._startedLoading.TryAdd(guid, Shared);
                }
                [HarmonyPatch(nameof(BlueprintsCache.RemoveCachedBlueprint)), HarmonyPostfix]
                internal static void RemoveCachedBlueprint(BlueprintGuid guid) {
                    lock (Shared.bpsToAdd) {
                        Shared.bpsToAdd.RemoveWhere(bp => bp.AssetGuid == guid);
                    }
                }
                internal static HashSet<BlueprintGuid> IsLoading = new();
                [HarmonyPatch(nameof(BlueprintsCache.Load)), HarmonyPrefix]
                public static bool Pre_Load(BlueprintGuid guid, ref SimpleBlueprint __result) {
                    if (!Shared.IsRunning) return true;
                    bool didAdd = Shared._startedLoading.TryAdd(guid, Shared);
                    if (didAdd) {
                        IsLoading.Add(guid);
                        return true;
                    } else {
                        lock (Shared._startedLoading[guid]) {
                            if (ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.TryGetValue(guid, out var entry)) {
                                __result = entry.Blueprint;
                            } else {
                                __result = null;
                            }
                        }
                        return false;
                    }
                }
                [HarmonyPatch(nameof(BlueprintsCache.Load)), HarmonyPostfix]
                public static void Post_Load(BlueprintGuid guid, ref SimpleBlueprint __result) {
                    if (IsLoading.Contains(guid)) {
                        IsLoading.Remove(guid);
                        lock (Shared.bpsToAdd) {
                            if (__result != null) Shared.bpsToAdd.Add(__result);
                        }
                    }
                }
            }
            [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init)), HarmonyFinalizer]
            internal static void BlueprintsCache_Init_Patch() {
                Shared.CanStart = true;
                if (Main.Settings.togglePreloadBlueprints || (Main.Settings.toggleUseBPIdCache && Main.Settings.toggleAutomaticallyBuildBPIdCache && BlueprintIdCache.NeedsCacheRebuilt)) Shared.GetBlueprints();
            }
            [HarmonyPatch]
            internal static class ReflectionBasedSerializer_PerformancePatches {
                private static Dictionary<(Type, Type), bool> HasAttributeCache = new();
                private static Dictionary<(Type, Type), bool> IsListOfCache = new();
                private static Dictionary<(Type, Type), bool> IsOrSubclassOfCache = new();
                private static Dictionary<Type, Func<object>> TypeConstructorCache = new();
                [HarmonyTargetMethods]
                internal static IEnumerable<MethodBase> GetMethods() {
                    foreach (var method in typeof(ReflectionBasedSerializer).GetMethods(AccessTools.all)) {
                        foreach (var instruction in PatchProcessor.GetCurrentInstructions(method) ?? new()) {
                            if (instruction.Calls(AccessTools.Method(typeof(ReflectionBasedSerializer), nameof(ReflectionBasedSerializer.CreateObject)))) {
                                yield return method;
                            } else if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo methodInfo) {
                                if (methodInfo.Name == "HasAttribute" && methodInfo.IsGenericMethod) {
                                    yield return method;
                                    continue;
                                } else if (methodInfo.Name == "IsListOf" && methodInfo.IsGenericMethod) {
                                    yield return method;
                                    continue;
                                } else if (methodInfo.Name == "IsOrSubclassOf" && methodInfo.IsGenericMethod) {
                                    yield return method;
                                    continue;
                                }
                            }
                        }
                    }
                }
                [HarmonyTranspiler, HarmonyDebug]
                internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    var method1 = AccessTools.Method(typeof(ReflectionBasedSerializer_PerformancePatches), nameof(HasAttribute));
                    var method2 = AccessTools.Method(typeof(ReflectionBasedSerializer_PerformancePatches), nameof(IsListOf));
                    var method3 = AccessTools.Method(typeof(ReflectionBasedSerializer_PerformancePatches), nameof(IsOrSubclassOf));
                    foreach (var instruction in instructions) {
                        if (instruction.Calls(AccessTools.Method(typeof(ReflectionBasedSerializer), nameof(ReflectionBasedSerializer.CreateObject)))) {
                            yield return CodeInstruction.Call((Type t) => CreateObject(t));
                            continue;
                        } else if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo methodInfo) {
                            if (methodInfo.Name == "HasAttribute" && methodInfo.IsGenericMethod) {
                                var genericArguments = methodInfo.GetGenericArguments();
                                yield return new CodeInstruction(OpCodes.Ldtoken, genericArguments[0]);
                                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Type), nameof(Type.GetTypeFromHandle)));
                                yield return new CodeInstruction(OpCodes.Call, method1);
                                continue;
                            } else if (methodInfo.Name == "IsListOf" && methodInfo.IsGenericMethod) {
                                var genericArguments = methodInfo.GetGenericArguments();
                                yield return new CodeInstruction(OpCodes.Ldtoken, genericArguments[0]);
                                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Type), nameof(Type.GetTypeFromHandle)));
                                yield return new CodeInstruction(OpCodes.Call, method2);
                                continue;
                            } else if (methodInfo.Name == "IsOrSubclassOf" && methodInfo.IsGenericMethod) {
                                var genericArguments = methodInfo.GetGenericArguments();
                                yield return new CodeInstruction(OpCodes.Ldtoken, genericArguments[0]);
                                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Type), nameof(Type.GetTypeFromHandle)));
                                yield return new CodeInstruction(OpCodes.Call, method3);
                                continue;
                            }
                        }
                        yield return instruction;
                    }
                }
                public static object CreateObject(Type type) {
                    if (!TypeConstructorCache.TryGetValue(type, out var f)) {
                        if (type.GetConstructor(Type.EmptyTypes) is { } constructor) {
                            f = TypeConstructorCache[type] =
                                Expression.Lambda<Func<object>>(
                                    Expression.Convert(
                                        Expression.New(constructor), typeof(object)))
                                .Compile();
                        } else {
                            f = TypeConstructorCache[type] = null;
                        }
                    }
                    if (f == null) return FormatterServices.GetUninitializedObject(type);
                    return f();
                }
                public static bool HasAttribute(Type t, Type T) {
                    if (!HasAttributeCache.TryGetValue((t, T), out var hasAttr)) {
                        hasAttr = t.GetCustomAttributes(T, true).Length != 0;
                        HasAttributeCache[(t, T)] = hasAttr;
                    }
                    return hasAttr;
                }
                public static bool IsListOf(Type t, Type T) {
                    if (!IsListOfCache.TryGetValue((t, T), out var isListOf)) {
                        isListOf = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) && t.GenericTypeArguments[0] == T;
                        IsListOfCache[(t, T)] = isListOf;
                    }
                    return isListOf;
                }
                public static bool IsOrSubclassOf(Type t, Type T) {
                    if (!IsOrSubclassOfCache.TryGetValue((t, T), out var isSubclassOf)) {
                        isSubclassOf = t == T || t.IsSubclassOf(T);
                        IsOrSubclassOfCache[(t, T)] = isSubclassOf;
                    }
                    return isSubclassOf;
                }
            }
        }
    }
}
