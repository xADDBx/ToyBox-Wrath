using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using ToyBox.Infrastructure;
using Kingmaker.EntitySystem.Entities;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.Cheats.InfiniteAbilitiesFeature")]
public partial class InfiniteAbilitiesFeature : FeatureWithPatch {
    public override ref bool IsEnabled => ref Settings.ToggleInfiniteAbilities;
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.InfiniteAbilitiesFeature";
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_InfiniteAbilitiesFeature_Name", "Infinite Abilities")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_InfiniteAbilitiesFeature_Description", "Prevents ability resources/usages from being spent")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(AbilityResourceLogic), nameof(AbilityResourceLogic.Spend)), HarmonyPrefix]
    private static bool AbilityResourceLogic_Spend_Patch(AbilityData ability) {
        return ShouldRunOriginal(ability.Caster.Unit);
    }
    [HarmonyPatch(typeof(ActivatableAbilityResourceLogic), nameof(ActivatableAbilityResourceLogic.SpendResource)), HarmonyPrefix]
    private static bool ActivatableAbilityResourceLogic_SpendResource_Patch(ActivatableAbilityResourceLogic __instance) {
        return ShouldRunOriginal(__instance.Owner);
    }
    private static bool ShouldRunOriginal(UnitEntityData? unit) {
        if (ToyBoxUnitHelper.IsPartyOrPet(unit)) {
            return false;
        }
        return true;
    }
}
