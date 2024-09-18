// borrowed shamelessly and enhanced from Bag of Tricks https://www.nexusmods.com/pathfinderkingmaker/mods/26, which is under the MIT License

using HarmonyLib;
//using Kingmaker.Controllers.GlobalMap;
//using Kingmaker.UI._ConsoleUI.Models;
//using Kingmaker.UI.RestCamp;
using System.Collections.Generic;
//using Kingmaker.UI._ConsoleUI.GroupChanger;
using UnityEngine;
using UnityModManager = UnityModManagerNet.UnityModManager;
using ModKit;

namespace ToyBox.BagOfPatches {
    internal static class ModUI {
        [HarmonyPatch(typeof(UnityModManager.UI))]
        internal static class UnityModManagerUIPatch {
            public static UnityModManager.UI UnityModMangerUI = null;
            private static readonly Dictionary<int, float> scrollOffsets = new() { };
            [HarmonyPatch(nameof(UnityModManager.UI.Update))]
            [HarmonyPostfix]
            private static void Update(UnityModManager.UI __instance, ref Rect ___mWindowRect, ref Vector2[] ___mScrollPosition, ref int ___tabId) {
                UnityModMangerUI = __instance;
                // save these in case we need them inside the mod
                //Logger.Log($"Rect: {___mWindowRect}");
                UI.ummRect = ___mWindowRect;
                UI.ummWidth = ___mWindowRect.width;
                UI.ummScrollPosition = ___mScrollPosition;
                UI.ummTabID = ___tabId;
            }
        }
    }
}
