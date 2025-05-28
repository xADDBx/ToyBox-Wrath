using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.AllowAnyUnitAsYourMountFeature")]
public partial class AllowAnyUnitAsYourMountFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.AllowAnyUnitAsYourMountFeature";
    public override ref bool IsEnabled => ref Settings.ToggleAllowAnyUnitAsYourMount;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_AllowAnyUnitAsYourMountFeature_Name", "Ride Any Unit As Your Mount")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_AllowAnyUnitAsYourMountFeature_Description", "Allows you to mount any unit, even if they are not a pet (this will likely look weird)")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(AbilityTargetIsSuitableMount), nameof(AbilityTargetIsSuitableMount.CanMount)), HarmonyPrefix]
    private static bool AbilityTargetIsSuitableMount_CanMount_Patch(UnitEntityData master, UnitEntityData pet, ref bool __result) {
        __result = true;
        return false;
    }
}
