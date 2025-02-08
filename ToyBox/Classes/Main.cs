using UnityEngine;
using UnityModManagerNet;

namespace ToyBox {
#if DEBUG
    [EnableReloading]
#endif
    internal static class Main {
        internal static readonly Harmony HarmonyInstance = new("ToyBox");
#pragma warning disable CS8618 // Field is set by Load method
        internal static UnityModManager.ModEntry ModEntry;
#pragma warning restore CS8618
        private static Exception? _caughtException = null;
        private static bool Load(UnityModManager.ModEntry modEntry) {
            try {
                ModEntry = modEntry;
#if DEBUG
                modEntry.OnUnload = OnUnload;
#endif
                modEntry.OnToggle = OnToggle;
                modEntry.OnShowGUI = OnShowGUI;
                modEntry.OnHideGUI = OnHideGUI;
                modEntry.OnGUI = OnGUI;
                modEntry.OnUpdate = OnUpdate;
                modEntry.OnSaveGUI = OnSaveGUI;

                Infrastructure.Localization.LocalizationManager.Enable();
            } catch (Exception ex) {
                Error(ex);
                return false;
            }
            return true;
        }
#if DEBUG
        private static bool OnUnload(UnityModManager.ModEntry modEntry) {
            return true;
        }
#endif
        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            return true;
        }
        [LocalizedString("Main.NREButton")]
        private static string label = "Cause a NullReferenceException";
        [LocalizedString("Main.ResetExceptionButton")]
        private static string resetLabel = "Reset";
        private static void OnGUI(UnityModManager.ModEntry modEntry) {
            if (_caughtException == null) {
                try {
                    if (GUILayout.Button(label)) {
                        ((object)null).ToString();
                    }
                } catch (Exception ex) {
                    Error(ex);
                    _caughtException = ex;
                }
            } else {
                GUILayout.Label(_caughtException.ToString());
                if (GUILayout.Button(resetLabel)) {
                    _caughtException = null;
                }
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
        }
        private static void OnShowGUI(UnityModManager.ModEntry modEntry) {
        }
        private static void OnHideGUI(UnityModManager.ModEntry modEntry) {
        }
        private static void OnUpdate(UnityModManager.ModEntry modEntry, float z) {
        }
    }
}