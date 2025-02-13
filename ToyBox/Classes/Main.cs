using UnityEngine;
using UnityModManagerNet;

namespace ToyBox; 
#if DEBUG
[EnableReloading]
#endif
public static partial class Main {
    internal static readonly Harmony HarmonyInstance = new("ToyBox");
    internal static UnityModManager.ModEntry ModEntry = null!;
    private static Exception? m_CaughtException = null;
    private static List<FeatureTab> m_FeatureTabs = new();
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

            m_FeatureTabs.Add(new Features.SettingsFeature.SettingsFeatureTab());
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
    [LocalizedString("Main_NREButton", "Cause a NullReferenceException")]
    private static partial string label { get; }
    [LocalizedString("Main_ResetExceptionButton", "Reset")]
    private static partial string resetLabel { get; }
    private static void OnGUI(UnityModManager.ModEntry modEntry) {
        if (m_CaughtException == null) {
            try {
                if (GUILayout.Button(label)) {
                    ((object)null).ToString();
                }
                Settings.SelectedTab = GUILayout.SelectionGrid(Settings.SelectedTab, m_FeatureTabs.Select(t => t.Name).ToArray(), 10);
                m_FeatureTabs[Settings.SelectedTab].OnGui();
            } catch (Exception ex) {
                Error(ex);
                m_CaughtException = ex;
            }
        } else {
            GUILayout.Label(m_CaughtException.ToString());
            if (GUILayout.Button(resetLabel)) {
                m_CaughtException = null;
            }
        }
    }

    private static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
        Settings.Save();
    }
    private static void OnShowGUI(UnityModManager.ModEntry modEntry) {
    }
    private static void OnHideGUI(UnityModManager.ModEntry modEntry) {
    }
    private static void OnUpdate(UnityModManager.ModEntry modEntry, float z) {
    }
}