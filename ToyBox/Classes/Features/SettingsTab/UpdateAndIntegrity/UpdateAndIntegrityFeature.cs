using Kingmaker;

namespace ToyBox.Features.SettingsFeatures.UpdateAndIntegrity;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.SettingsFeatures.UpdateAndIntegrity.UpdateCompatabilityFeature")]
public partial class UpdateCompatabilityFeature : FeatureWithPatch, INeedEarlyInitFeature {
    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_UpdateCompatabilityFeature_Name", "Enable Version Compatibility Check")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_UpdateCompatabilityFeature_Description", "Check whether the current mod version is compatible with the current game version")]
    public override partial string Description { get; }

    public override ref bool IsEnabled => ref Settings.EnableVersionCompatibilityCheck;

    protected override string HarmonyName => "ToyBox.Features.SettingsFeatures.UpdateAndIntegrity.UpdateCompatabilityFeature";
    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Awake)), HarmonyPrefix]
    private static void MainMenu_Awake_Prefix() {
        if (VersionChecker.ResultOfCheck.HasValue && !VersionChecker.ResultOfCheck.Value) {
            Main.ModEntry.Info.DisplayName = "ToyBox ".Yellow().SizePercent(20) + ModVersionIsNotCompatibleWithThi.Red().Bold().SizePercent(40);
            Main.ModEntry.mErrorOnLoading = true;
            Main.ModEntry.OnUnload(Main.ModEntry);
            Main.ModEntry.OnGUI = Updater.UpdaterGUI;
        }
    }
    [LocalizedString("ToyBox_Features_SettingsFeatures_UpdateAndIntegrity_UpdateCompatabilityFeature_ModVersionIsNotCompatibleWithThi", "Mod Version is not compatible with this game version!")]
    private static partial string ModVersionIsNotCompatibleWithThi { get; }
}
