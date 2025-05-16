using Kingmaker.Blueprints;
using System.Collections.Concurrent;
using System.Diagnostics;
using ToyBox.Features.SettingsFeatures.UpdateAndIntegrity;
using ToyBox.Infrastructure;
using ToyBox.Infrastructure.Utilities;
using UnityEngine;
using UnityModManagerNet;

namespace ToyBox;
#if DEBUG
[EnableReloading]
#endif
public static partial class Main {
    [LocalizedString("ToyBox_Main_ModFilesAreCorrupted_Text", "Mod files are corrupted!")]
    private static partial string ModFilesAreCorrupted_Text { get; }
    internal static Harmony HarmonyInstance = new("ToyBox");
    internal static UnityModManager.ModEntry ModEntry = null!;
    internal static List<Task> LateInitTasks = new List<Task>();
    private static Exception? m_CaughtException = null;
    private static List<FeatureTab> m_FeatureTabs = new();
    private static readonly ConcurrentQueue<Action> m_MainThreadTaskQueue = new();
    private static bool Load(UnityModManager.ModEntry modEntry) {
        Stopwatch sw = Stopwatch.StartNew();
        try {
            ModEntry = modEntry;
            ModEntry.OnUnload = OnUnload;
            ModEntry.OnToggle = OnToggle;
            ModEntry.OnShowGUI = OnShowGUI;
            ModEntry.OnHideGUI = OnHideGUI;
            ModEntry.OnGUI = OnGUI;
            ModEntry.OnFixedUpdate = OnFixedUpdate;
            ModEntry.OnSaveGUI = OnSaveGUI;

            if (Settings.EnableFileIntegrityCheck && !IntegrityCheckerFeature.CheckFilesHealthy()) {
                Critical("Failed Integrity Check. Files have issues!");
                ModEntry.Info.DisplayName = "ToyBox ".Orange().SizePercent(40) + ModFilesAreCorrupted_Text.Red().Bold().SizePercent(60);
                ModEntry.OnGUI = _ => UpdaterFeature.UpdaterGUI();
                return true;
            }

            if (Settings.EnableVersionCompatibilityCheck) {
                Task.Run(() => {
                    var versionTimer = Stopwatch.StartNew();
                    VersionChecker.IsGameVersionSupported();
                    Debug($"Finished Version Compatibility Check in: {versionTimer.ElapsedMilliseconds}ms (Threaded)");
                });
            }

            Stopwatch sw2 = Stopwatch.StartNew();
            Infrastructure.Localization.LocalizationManager.Enable();
            Debug($"Localization init took {sw2.ElapsedMilliseconds}ms");

            sw2.Start();
            _ = BPLoader;
            Debug($"BPLoader init took {sw2.ElapsedMilliseconds}ms");

            sw2.Start();
            RegisterFeatureTabs();
            Debug($"Early init took {sw2.ElapsedMilliseconds}ms");

            foreach (var tab in m_FeatureTabs) {
                LateInitTasks.Add(Task.Run(tab.InitializeAll));
            }
            LazyInit.Stopwatch.Start();

            LazyInit.EnsureFinish();

        } catch (Exception ex) {
            Error(ex);
            return false;
        }
        Debug($"Complete init took {sw.ElapsedMilliseconds}ms");
        return true;
    }
    private static void RegisterFeatureTabs() {
        m_FeatureTabs.Add(new Features.BagOfTricks.BagOfTricksFeatureTab());
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
    [LocalizedString("ToyBox_Main_CurrentlyLoadedBPsText", "Currently loaded blueprints: {0}")]
    private static partial string CurrentlyLoadedBPsText { get; }
    private static int m_LoadedBps = 0;
    private static Browser<SimpleBlueprint> m_Browser = new(BPHelper.GetSortKey, BPHelper.GetSearchKey, [], (Action<IEnumerable<SimpleBlueprint>> func) => BPLoader.GetBlueprints(func));
    private static void OnGUI(UnityModManager.ModEntry modEntry) {
        if (m_CaughtException == null) {
            try {
                if (BPLoader.IsLoading) {
                    UI.ProgressBar(BPLoader.Progress, "");
                }
                if (ImguiCanChangeStateAtBeginning() && BPLoader.HasLoaded) {
                    m_LoadedBps = BPLoader.GetBlueprints()!.Count;
                }
                Space(10);
                Div.DrawDiv();
                Space(10);

                /*
                using (HorizontalScope()) {
                    Space(20);
                    m_Browser.OnGUI(item => {
                        UI.Label(BPHelper.GetTitle(item).Green());
                    });
                }
                */


                UI.Label(CurrentlyLoadedBPsText.Format(m_LoadedBps));
                Settings.SelectedTab = GUILayout.SelectionGrid(Settings.SelectedTab, m_FeatureTabs.Select(t => t.Name).ToArray(), 10);
                Space(10);
                Div.DrawDiv();
                Space(10);
                m_FeatureTabs[Settings.SelectedTab].OnGui();
            } catch (Exception ex) {
                Error(ex);
                m_CaughtException = ex;
            }
        } else {
            UI.Label(m_CaughtException.ToString());
            if (UI.Button(resetLabel)) {
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
        Settings.Save();
    }
    private static void OnFixedUpdate(UnityModManager.ModEntry modEntry, float z) {
        try {
            if (m_MainThreadTaskQueue.TryDequeue(out var task)) {
                task();
            }
        } catch (Exception ex) {
            Error(ex);
        }
    }
    public static void ScheduleForMainThread(this Action action) {
        m_MainThreadTaskQueue.Enqueue(action);
    }
}
