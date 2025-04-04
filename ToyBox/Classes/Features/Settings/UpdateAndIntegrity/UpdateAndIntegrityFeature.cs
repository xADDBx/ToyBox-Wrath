using Kingmaker;
using UnityEngine;

namespace ToyBox.Features.UpdateAndIntegrity;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.UpdateAndIntegrity.UpdateAndIntegrityFeature")]
public partial class UpdateAndIntegrityFeature : FeatureWithPatch, INeedEarlyInitFeature {
    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_UpdateAndIntegrityFeature_Name", "Update, Integrity and Version Checker")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_UpdateAndIntegrityFeature_Description", "Check for updates, file integrity and version compatibility.")]
    public override partial string Description { get; }

    public override bool IsEnabled => Settings.EnableVersionCompatibilityCheck;

    protected override string HarmonyName => "ToyBox.Features.UpdateAndIntegrity.UpdateAndIntegrityFeature";
    public override void OnGui() {
        using (VerticalScope()) {
            using (HorizontalScope()) {
                bool newValue = GUILayout.Toggle(Settings.EnableVersionCompatibilityCheck, EnableVersionCompatibilityCheckT.Cyan(), GUILayout.ExpandWidth(false));
                if (newValue != Settings.EnableVersionCompatibilityCheck) {
                    if (Settings.EnableVersionCompatibilityCheck) {
                        Unpatch();
                    } else {
                        Patch();
                    }
                    Settings.EnableVersionCompatibilityCheck = newValue;
                }
                GUILayout.Space(10);
                GUILayout.Label(CheckWhetherTheCurrentModVersion.Green(), GUILayout.ExpandWidth(false));
            }
            using (HorizontalScope()) {
                bool newValue = GUILayout.Toggle(Settings.EnableFileIntegrityCheck, EnableIntegrityCheckText.Cyan(), GUILayout.ExpandWidth(false));
                if (newValue != Settings.EnableFileIntegrityCheck) {
                    Settings.EnableFileIntegrityCheck = newValue;
                }
                GUILayout.Space(10);
                GUILayout.Label(CheckTheIntegrityOfToyBoxFilesTe.Green(), GUILayout.ExpandWidth(false));
            }
        }
    }
    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Awake)), HarmonyPrefix]
    private static void MainMenu_Awake_Prefix() {
        if (VersionChecker.ResultOfCheck.HasValue && !VersionChecker.ResultOfCheck.Value) {
            Main.ModEntry.Info.DisplayName = "ToyBox ".Yellow().SizePercent(20) + ModVersionIsNotCompatibleWithThi.Red().Bold().SizePercent(40);
            Main.ModEntry.mErrorOnLoading = true;
            Main.ModEntry.OnUnload(Main.ModEntry);
            Main.ModEntry.OnGUI = Updater.UpdaterGUI;
        }
    }
    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_UpdateAndIntegrityFeature_ModVersionIsNotCompatibleWithThi", "Mod Version is not compatible with this game version!")]
    private static partial string ModVersionIsNotCompatibleWithThi { get; }
    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_UpdateAndIntegrityFeature_EnableVersionCompatibilityCheckT", "Enable Version Compatibility Check")]
    private static partial string EnableVersionCompatibilityCheckT { get; }
    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_UpdateAndIntegrityFeature_CheckWhetherTheCurrentModVersion", "Check whether the current mod version is compatible with the current game version")]
    private static partial string CheckWhetherTheCurrentModVersion { get; }
    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_UpdateAndIntegrityFeature_EnableIntegrityCheckText", "Enable Integrity Check")]
    private static partial string EnableIntegrityCheckText { get; }
    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_UpdateAndIntegrityFeature_CheckTheIntegrityOfToyBoxFilesTe", "Check the integrity of ToyBox files")]
    private static partial string CheckTheIntegrityOfToyBoxFilesTe { get; }
}
