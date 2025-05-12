using Kingmaker.View.MapObjects.Traps;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.Cheats.PreventTrapsFromTriggeringFeature")]
public partial class PreventTrapsFromTriggeringFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.PreventTrapsFromTriggeringFeature";
    public override ref bool IsEnabled => ref Settings.DisableTraps;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_PreventTrapsFromTriggeringFeature_Name", "Prevent Traps from triggering")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_PreventTrapsFromTriggeringFeature_Description", "Entering a Trap Zone while having Traps disabled will prevent that Trap from triggering even if you deactivate this option in the future")]
    public override partial string Description { get; }

    [HarmonyPatch(typeof(TrapObjectData), nameof(TrapObjectData.TryTriggerTrap)), HarmonyPrefix]
    private static bool TrapObjectData_TryTriggerTrap_Patch() {
        return false;
    }
}
