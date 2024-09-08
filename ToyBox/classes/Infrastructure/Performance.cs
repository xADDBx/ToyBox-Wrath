using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.BinaryFormat;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ToyBox {
    //[HarmonyPatch]
    public static class Performance {
        [HarmonyPatch]
        internal static class ReflectionBasedSerializer_PerformancePatches {
            private static Dictionary<(Type, Type), bool> HasAttributeCache = new();
            private static Dictionary<(Type, Type), bool> IsListOfCache = new();
            private static Dictionary<(Type, Type), bool> IsListCache = new();
            private static Dictionary<(Type, Type), bool> IsOrSubclassOfCache = new();
            private static Dictionary<Type, Func<object>> TypeConstructorCache = new();
            private static Dictionary<Type, Func<int, Array>> ArrayTypeConstructorCache = new();
            private static MethodInfo Activator_CreateInstance = AccessTools.Method(typeof(Activator), nameof(Activator.CreateInstance), [typeof(Type)]);
            private static MethodInfo Array_CreateInstance = AccessTools.Method(typeof(Array), nameof(Array.CreateInstance), [typeof(Type), typeof(int)]);
            private static MethodInfo ReflectionBasedSerializer_CreateObject = AccessTools.Method(typeof(ReflectionBasedSerializer), nameof(ReflectionBasedSerializer.CreateObject));
            [HarmonyTargetMethods]
            internal static IEnumerable<MethodBase> GetMethods() {
                var ret = new List<MethodBase>();
                foreach (var method in typeof(ReflectionBasedSerializer).GetMethods(AccessTools.all).Concat(typeof(PrimitiveSerializer).GetMethods(AccessTools.all)).Concat(typeof(BlueprintFieldsTraverser).GetMethods(AccessTools.all)).Concat(typeof(FieldsContractResolver).GetMethods(AccessTools.all))) {
                    try {
                        foreach (var instruction in PatchProcessor.GetCurrentInstructions(method) ?? new()) {
                            if (instruction.Calls(Activator_CreateInstance)) {
                                ret.Add(method);
                                continue;
                            } else if (instruction.Calls(Array_CreateInstance)) {
                                ret.Add(method);
                                continue;
                            } else if (instruction.Calls(ReflectionBasedSerializer_CreateObject)) {
                                ret.Add(method);
                                continue;
                            } else if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo methodInfo) {
                                if (methodInfo.Name == "HasAttribute" && methodInfo.IsGenericMethod) {
                                    ret.Add(method);
                                    continue;
                                } else if (methodInfo.Name == "IsListOf" && methodInfo.IsGenericMethod) {
                                    ret.Add(method);
                                    continue;
                                } else if (methodInfo.Name == "IsList" && methodInfo.IsGenericMethod) {
                                    ret.Add(method);
                                    continue;
                                } else if (methodInfo.Name == "IsOrSubclassOf" && methodInfo.IsGenericMethod) {
                                    ret.Add(method);
                                    continue;
                                }
                            }
                        }
                    } catch { }
                }
                ret.Add(AccessTools.Method(typeof(GuidClassBinder), nameof(GuidClassBinder.IsIdentifiedType)));
                return ret;
            }
            [HarmonyTranspiler]
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                var method1 = AccessTools.Method(typeof(ReflectionBasedSerializer_PerformancePatches), nameof(HasAttribute));
                var method2 = AccessTools.Method(typeof(ReflectionBasedSerializer_PerformancePatches), nameof(IsListOf));
                var method3 = AccessTools.Method(typeof(ReflectionBasedSerializer_PerformancePatches), nameof(IsList));
                var method4 = AccessTools.Method(typeof(ReflectionBasedSerializer_PerformancePatches), nameof(IsOrSubclassOf));
                foreach (var instruction in instructions) {
                    if (instruction.Calls(Activator_CreateInstance)) {
                        yield return CodeInstruction.Call((Type t) => CreateInstance(t));
                        continue;
                    } else if (instruction.Calls(Array_CreateInstance)) {
                        yield return CodeInstruction.Call((Type t, int n) => CreateArrayInstance(t, n));
                        continue;
                    } else if (instruction.Calls(ReflectionBasedSerializer_CreateObject)) {
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
                        } else if (methodInfo.Name == "IsList" && methodInfo.IsGenericMethod) {
                            var genericArguments = methodInfo.GetGenericArguments();
                            yield return new CodeInstruction(OpCodes.Ldtoken, genericArguments[0]);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Type), nameof(Type.GetTypeFromHandle)));
                            yield return new CodeInstruction(OpCodes.Call, method3);
                            continue;
                        } else if (methodInfo.Name == "IsOrSubclassOf" && methodInfo.IsGenericMethod) {
                            var genericArguments = methodInfo.GetGenericArguments();
                            yield return new CodeInstruction(OpCodes.Ldtoken, genericArguments[0]);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Type), nameof(Type.GetTypeFromHandle)));
                            yield return new CodeInstruction(OpCodes.Call, method4);
                            continue;
                        }
                    }
                    yield return instruction;
                }
            }
            public static object CreateInstance(Type type) {
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
                if (f == null) return Activator.CreateInstance(type);
                return f();
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
            public static object CreateArrayInstance(Type type, int length) {
                if (!ArrayTypeConstructorCache.TryGetValue(type, out var f)) {
                    var param = Expression.Parameter(typeof(int), "length");
                    var newArrayExpression = Expression.NewArrayBounds(type, param);
                    var lambda = Expression.Lambda<Func<int, Array>>(newArrayExpression, param);

                    f = ArrayTypeConstructorCache[type] = lambda.Compile();
                }
                if (f == null) return Array.CreateInstance(type, length);
                return f(length);
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
            public static bool IsList(Type t, Type T) {
                if (!IsListCache.TryGetValue((t, T), out var isList)) {
                    isList = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>);
                    IsListCache[(t, T)] = isList;
                }
                return isList;
            }
            public static bool IsOrSubclassOf(Type t, Type T) {
                if (!IsOrSubclassOfCache.TryGetValue((t, T), out var isSubclassOf)) {
                    isSubclassOf = t == T || t.IsSubclassOf(T);
                    IsOrSubclassOfCache[(t, T)] = isSubclassOf;
                }
                return isSubclassOf;
            }
        }
        /* Speeds up ~1s; Needs cache clearing to be implemented
        private static Dictionary<CharacterStats, Dictionary<StatType, ModifiableValueAttributeStat>> AttributeCache = new();
        [HarmonyPatch(typeof(CharacterStats), nameof(CharacterStats.GetAttribute)), HarmonyPrefix]
        public static bool GetAttribute(CharacterStats __instance, StatType type, ref ModifiableValueAttributeStat __result) {
            if (!AttributeCache.TryGetValue(__instance, out var cache)) {
                if (__instance.Attributes == null) {
                    __result = null;
                    return false;
                }
                Dictionary<StatType, ModifiableValueAttributeStat> tmp = new();
                foreach (var stat in __instance.Attributes) {
                    tmp[stat.Type] = stat;
                }
                AttributeCache[__instance] = cache = tmp;
            }
            cache.TryGetValue(type, out __result);
            return false;
        }
        [HarmonyPatch]
        public static class GetStatPatch {
            private static Dictionary<CharacterStats, Dictionary<StatType, ModifiableValue>> StatCache = new();
            [HarmonyTargetMethod]
            public static MethodBase TargetMethod() {
                return typeof(CharacterStats).GetMethods().First(mi => !mi.IsGenericMethod && mi.Name == nameof(CharacterStats.GetStat));
            }
            [HarmonyPrefix]
            public static bool GetStat(CharacterStats __instance, StatType type, ref ModifiableValue __result) {
                if (!StatCache.TryGetValue(__instance, out var cache)) {
                    if (__instance.Attributes == null) {
                        __result = null;
                        return false;
                    }
                    Dictionary<StatType, ModifiableValue> tmp = new();
                    foreach (var stat in __instance.AllStats) {
                        tmp[stat.Type] = stat;
                    }
                    StatCache[__instance] = cache = tmp;
                }
                cache.TryGetValue(type, out __result);
                return false;
            }
        }
        */
        /* Sped up save loading for me by up to 5~6s under profiling
        private static bool IsLoading = false;
        private static HashSet<string> tmp = new();
        [HarmonyPatch(typeof(LoadingProcess), nameof(LoadingProcess.TickLoading)), HarmonyPrefix]
        public static void TickLoadingPre(LoadingProcess __instance) {
            IsLoading = __instance.m_CurrentLoadingProcess != null;
        }
        [HarmonyPatch(typeof(LoadingProcess), nameof(LoadingProcess.TickLoading)), HarmonyPostfix]
        public static void TickLoadingPost(LoadingProcess __instance) {
            if (IsLoading != (__instance.m_CurrentLoadingProcess != null)) {
                Main.Settings.savePreloadHelper = tmp;
            }
        }
        [HarmonyPatch]
        public static class TryGetBlueprintPatch {
            [HarmonyPrepare]
            public static bool Prepare() => Main.Settings.togglePreloadSaveBlueprints;
            [HarmonyTargetMethod]
            public static MethodBase TargetMethod() {
                return typeof(ResourcesLibrary).GetMethods().First(mi => !mi.IsGenericMethod && mi.Name == nameof(ResourcesLibrary.TryGetBlueprint) && mi.GetParameters()?.Length == 1 && mi.GetParameters()[0].ParameterType == typeof(BlueprintGuid));
            }
            [HarmonyPrefix]
            public static void TryGetBlueprint(BlueprintGuid assetId) {
                if (IsLoading) {
                    tmp.Add(assetId.ToString());
                }
            }
        }
        */
    }
}
