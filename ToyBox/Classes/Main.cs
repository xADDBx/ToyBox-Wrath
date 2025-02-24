using ToyBox.Infrastructure.Utilities;
using ToyBox.UpdateAndIntegrity;
using UnityEngine;
using UnityModManagerNet;

namespace ToyBox;
#if DEBUG
[EnableReloading]
#endif
public static partial class Main {
    internal static Harmony HarmonyInstance = new("ToyBox");
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
            /*
            if (!IntegrityChecker.CheckFilesHealthy()) {
                Log("Failed Integrity Check");
            }
            if (!VersionChecker.IsGameVersionSupported()) {
                Log("Game Version not supported");
            }
            if (Updater.Update(false, true)) {
                Log("Updated");
            }
            */

            Infrastructure.Localization.LocalizationManager.Enable();
            _ = BPLoader;

            RegisterFeatureTabs();
            foreach (var tab in m_FeatureTabs) {
                tab.InitializeAll();
            }
        } catch (Exception ex) {
            Error(ex);
            return false;
        }
        return true;
    }
    private static void RegisterFeatureTabs() {
        m_FeatureTabs.Add(new Features.SettingsFeature.SettingsFeatureTab());
    }
#if DEBUG
    private static bool OnUnload(UnityModManager.ModEntry modEntry) {
        foreach (var tab in m_FeatureTabs) {
                tab.DestroyAll();
        }
        HarmonyInstance.UnpatchAll(ModEntry.Info.Id);
        return true;
    }
#endif
    private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
        return true;
    }
    [LocalizedString("ToyBox_Main_CauseANullReferenceExceptionText", "Cause a NullReferenceException")]
    private static partial string label { get; }
    [LocalizedString("ToyBox_Main_ResetText", "Reset")]
    private static partial string resetLabel { get; }
    [LocalizedString("ToyBox_Main_LoadBlueprintsText", "Load Blueprints")]
    private static partial string LoadBlueprintsText { get; }
    [LocalizedString("ToyBox_Main_CurrentlyLoadedBPsText", "Currently loaded BPs: {0}")]
    private static partial string CurrentlyLoadedBPsText { get; }
    private static int m_LoadedBps = 0;
    private static bool m_WasBPLoadingLastFrame = false;
    private static void OnGUI(UnityModManager.ModEntry modEntry) {
        if (m_CaughtException == null) {
            try {
                if (BPLoader.IsLoading || m_WasBPLoadingLastFrame) {
                    GUILayout.Label($"{BPLoader.Progress * 100:0.00}%");
                    if (Event.current.type == EventType.Layout) {
                        m_WasBPLoadingLastFrame = true;
                    } else {
                        m_WasBPLoadingLastFrame = false;
                    }
                }
                if (GUILayout.Button(label)) {
                    ((object)null).ToString();
                }
                if (GUILayout.Button(LoadBlueprintsText)) {
                    BPLoader.GetBlueprints();
                }
                if (Event.current.type == EventType.Layout && BPLoader.HasLoaded) {
                    m_LoadedBps = BPLoader.GetBlueprints()!.Count;
                }
                GUILayout.Label(CurrentlyLoadedBPsText.Format(m_LoadedBps));
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