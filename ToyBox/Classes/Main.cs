using System.Diagnostics;
using ToyBox.Features.UpdateAndIntegrity;
using ToyBox.Infrastructure.UI;
using ToyBox.Infrastructure.Utilities;
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
            ModEntry.OnUnload = OnUnload;
            ModEntry.OnToggle = OnToggle;
            ModEntry.OnShowGUI = OnShowGUI;
            ModEntry.OnHideGUI = OnHideGUI;
            ModEntry.OnGUI = OnGUI;
            ModEntry.OnUpdate = OnUpdate;
            ModEntry.OnSaveGUI = OnSaveGUI;

            if (Settings.EnableFileIntegrityCheck && !IntegrityChecker.CheckFilesHealthy()) {
                Critical("Failed Integrity Check. Files have issues!"); 
                ModEntry.Info.DisplayName = "ToyBox ".Orange().SizePercent(40) + ModFilesAreCorrupted_Text.Red().Bold().SizePercent(60);
                // ModEntry.mErrorOnLoading = true;
                ModEntry.OnGUI = Updater.UpdaterGUI;
                return true;
            }

            if (Settings.EnableVersionCompatibilityCheck) {
                Task.Run(() => {
                    var versionTimer = Stopwatch.StartNew();
                    VersionChecker.IsGameVersionSupported();
                    Debug($"Finished ToyBox Version Compatibility Check in: {versionTimer.ElapsedMilliseconds}ms (Threaded)");
                });
            }

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
        m_FeatureTabs.Add(new Features.SettingsFeatures.SettingsFeaturesTab());
    }
    private static bool OnUnload(UnityModManager.ModEntry modEntry) {
        foreach (var tab in m_FeatureTabs) {
                tab.DestroyAll();
        }
        HarmonyInstance.UnpatchAll(ModEntry.Info.Id);
        return true;
    }
    private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
        return true;
    }
    [LocalizedString("ToyBox_Main_ResetText", "Reset")]
    private static partial string resetLabel { get; }
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
                if (ImguiCanChangeStateAtBeginning() && BPLoader.HasLoaded) {
                    m_LoadedBps = BPLoader.GetBlueprints()!.Count;
                }
                GUILayout.Label(CurrentlyLoadedBPsText.Format(m_LoadedBps));
                Settings.SelectedTab = GUILayout.SelectionGrid(Settings.SelectedTab, m_FeatureTabs.Select(t => t.Name).ToArray(), 10);
                GUILayout.Space(10);
                Div.DrawDiv();
                GUILayout.Space(10);
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

    [LocalizedString("ToyBox_Main_ModFilesAreCorrupted_Text", "Mod files are corrupted!")]
    private static partial string ModFilesAreCorrupted_Text { get; }
}
