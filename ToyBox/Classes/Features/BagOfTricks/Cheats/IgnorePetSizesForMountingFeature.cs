using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.Utility;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.IgnorePetSizesForMountingFeature")]
public partial class IgnorePetSizesForMountingFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.IgnorePetSizesForMountingFeature";
    public override ref bool IsEnabled => ref Settings.ToggleIgnorePetSizesForMounting;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_IgnorePetSizesForMountingFeature_Name", "Ignore pet sizes for mounting")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_IgnorePetSizesForMountingFeature_Description", "Allows mounting pets of any size")]
    public override partial string Description { get; }
    private const string MountTargetAbility = "MountTargetAbility";
    [HarmonyPatch(typeof(AbilityTargetHasFact), nameof(AbilityTargetHasFact.IsTargetRestrictionPassed)), HarmonyPrefix]
    public static bool AbilityTargetHasFact_IsTargetRestrictionPassed_Patch(AbilityTargetHasFact __instance, UnitEntityData caster, TargetWrapper target, ref bool __result) {
        if (__instance.OwnerBlueprint.AssetGuid == MountTargetAbility) {
            __result = true;
            return false;
        }
        return true;
    }
    [HarmonyPatch(typeof(AbilityTargetIsSuitableMountSize), nameof(AbilityTargetIsSuitableMountSize.CanMount)), HarmonyPrefix]
    private static bool AbilityTargetIsSuitableMountSize_CanMount_Patch(UnitEntityData master, UnitEntityData pet, ref bool __result) {
        __result = true;
        return false;
    }
}
