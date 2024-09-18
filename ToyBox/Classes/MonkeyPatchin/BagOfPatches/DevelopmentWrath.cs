using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using Kingmaker.UI.MVVM._VM.CharGen;
using Kingmaker.Utility;
using ModKit;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using UnityModManagerNet;

namespace ToyBox.classes.MonkeyPatchin.BagOfPatches {
    internal class Development {
        public static Settings settings => Main.Settings;

        [HarmonyPatch(typeof(BuildModeUtility), nameof(BuildModeUtility.IsDevelopment), MethodType.Getter)]
        private static class BuildModeUtility_IsDevelopment_Patch {
            private static void Postfix(ref bool __result) {
                if (settings.toggleDevopmentMode) __result = true;
            }
        }

        [HarmonyPatch(typeof(SmartConsole), nameof(SmartConsole.WriteLine))]
        private static class SmartConsole_WriteLine_Patch {
            private static void Postfix(string? message) {
                if (settings.toggleDevopmentMode) {
                    Mod.Log(message);
                    var timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    UberLoggerAppWindow.Instance.Log(new LogInfo(null, LogChannel.Default, timestamp, LogSeverity.Message, new List<LogStackFrame>(), false, message, Array.Empty<object>())
                        );
                }
            }
        }
        // For some reason patching Logger.Write directly will cause a crash for a very small subset of users if they have Doorstop UMM
        // While switching to Assembly works; using this seems to be a proper workaround?
        [HarmonyPatch(typeof(UnityModManager.Logger))]
        private static class Logger_Logger_Patch {
            [HarmonyPatch(nameof(UnityModManager.Logger.NativeLog), [typeof(string), typeof(string)])]
            [HarmonyPrefix]
            private static void NativeLog(ref string str) {
                try {
                    if (settings.stripHtmlTagsFromNativeConsole) str = str.StripHTML();
                } catch { }
            }
            [HarmonyPatch(nameof(UnityModManager.Logger.Log), [typeof(string), typeof(string)])]
            [HarmonyPrefix]
            private static void Log(ref string str) {
                try {
                    if (settings.stripHtmlTagsFromUMMLogsTab) str = str.StripHTML();
                } catch { }
            }
            [HarmonyPatch(nameof(UnityModManager.Logger.Error), [typeof(string), typeof(string)])]
            [HarmonyPrefix]
            private static void Error(ref string str) {
                try {
                    if (settings.stripHtmlTagsFromUMMLogsTab) str = str.StripHTML();
                } catch { }
            }
        }

        [HarmonyPatch(typeof(CharGenContextVM), nameof(CharGenContextVM.HandleRespecInitiate))]
        private static class CharGenContextVM_HandleRespecInitiate_Patch {
            private static void Prefix(ref CharGenContextVM __instance, ref UnitEntityData character, ref Action successAction) {
                if (settings.toggleRespecRefundScrolls) {
                    var scrolls = new List<BlueprintItemEquipmentUsable>();

                    var loadedscrolls = Game.Instance.BlueprintRoot.CraftRoot.m_ScrollsItems.Select(a => ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentUsable>(a.Guid));
                    foreach (var spellbook in character.Spellbooks) {
                        foreach (var scrollspell in spellbook.GetAllKnownSpells())
                            if (scrollspell.CopiedFromScroll)
                                if (loadedscrolls.TryFind(a => a.Ability.NameForAcronym == scrollspell.Blueprint.NameForAcronym, out var item))
                                    scrolls.Add(item);
                    }

                    successAction = PatchedSuccessAction(successAction, scrolls);
                }
            }

            private static Action PatchedSuccessAction(Action successAction, List<BlueprintItemEquipmentUsable> scrolls) =>
                () => {
                    foreach (var scroll in scrolls) Game.Instance.Player.Inventory.Add(new ItemEntityUsable(scroll));
                    successAction.Invoke();
                };
        }

        [HarmonyPatch(typeof(BlueprintConverter), nameof(BlueprintConverter.ReadJson))]
        private static class ForceSuccessfulLoad_Blueprints_Patch {
            private static bool Prefix(ref object? __result, JsonReader reader) {
                if (!settings.enableLoadWithMissingBlueprints) return true;
                var text = (string)reader.Value;
                if (string.IsNullOrEmpty(text) || text == "null") {
                    //Mod.Warn($"ForceSuccessfulLoad_Blueprints_Patch - unable to find valid id - text: {text}");
                    __result = null; // We still can't look up a blueprint without a valid id
                    return false;
                }
                SimpleBlueprint retrievedBlueprint;
                try {
                    retrievedBlueprint = ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(text));
                } catch {
                    retrievedBlueprint = null;
                }
                if (retrievedBlueprint == null) Mod.Warn($"Failed to load blueprint by guid '{text}' but continued with null blueprint.");
                __result = retrievedBlueprint;

                return false;
            }
        }

        [HarmonyPatch(typeof(EntityFact), nameof(EntityFact.ComponentsDictionary), MethodType.Setter)]
        private static class ForceSuccessfulLoad_OfFacts_Patch {
            private static void Prefix(ref EntityFact __instance) {
                if (__instance.Blueprint == null) Mod.Warn($"Fact type '{__instance}' failed to load. UniqueID: {__instance.UniqueId}");
            }
        }
    }
}