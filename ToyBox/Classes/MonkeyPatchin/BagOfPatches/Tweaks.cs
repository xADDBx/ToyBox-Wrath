using HarmonyLib;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.AreaLogic.Etudes;
using System;
using Kingmaker.Controllers;

namespace ToyBox.BagOfPatches {
    internal static partial class Tweaks {
        [HarmonyPatch(typeof(TrapObjectData), nameof(TrapObjectData.TryTriggerTrap))]
        public static class TrapObjectData_TryTriggerTrap_Patch {
            private static bool Prefix() {
                if (Settings.disableTraps) {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(BlueprintEtude), nameof(BlueprintEtude.IsReadOnly), MethodType.Getter)]
        public static class BlueprintEtude_IsReadOnly_Patch {
            private static void Postfix(ref bool __result) {
                if (Settings.allEtudesReadable) {
                    __result = false;
                }
            }
        }
        [HarmonyPatch(typeof(TimeController))]
        public static class TimeController_Patch {
            [HarmonyPatch(MethodType.Constructor)]
            [HarmonyPostfix]
            public static void Const(TimeController __instance) {
                var timeScale = Main.Settings.useAlternateTimeScaleMultiplier
                    ? Main.Settings.alternateTimeScaleMultiplier
                    : Main.Settings.timeScaleMultiplier;
                __instance.DebugTimeScale = timeScale;
            }
        }
    }
}