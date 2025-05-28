using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Parts;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.IgnoreAlignmentRequirementsForAbilitiesFeature")]
public partial class IgnoreAlignmentRequirementsForAbilitiesFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.IgnoreAlignmentRequirementsForAbilitiesFeature";
    public override ref bool IsEnabled => ref Settings.ToggleIgnoreAlignmentRequirementsForAbilities;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_IgnoreAlignmentRequirementsForAbilitiesFeature_Name", "Ignore Alignment Requirements for Abilities")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_IgnoreAlignmentRequirementsForAbilitiesFeature_Description", "Disables ability/spellbook checks regarding alignment for both initiator and target")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(AbilityCasterAlignment), nameof(AbilityCasterAlignment.IsCasterRestrictionPassed)), HarmonyPostfix]
    private static void AbilityCasterAlignment_IsCasterRestrictionPassed_Patch(ref bool __result) {
        __result = true;
    }

    [HarmonyPatch(typeof(UnitPartForbiddenSpellbooks), nameof(UnitPartForbiddenSpellbooks.IsForbidden)), HarmonyPostfix]
    public static void UnitPartForbiddenSpellbooks_IsForbidden_Patch(ref bool __result) {
        __result = false;
    }

    [HarmonyPatch(typeof(UnitPartForbiddenSpellbooks), nameof(UnitPartForbiddenSpellbooks.Add)), HarmonyPrefix]
    public static bool UnitPartForbiddenSpellbooks_Add_Patch(ForbidSpellbookReason reason) {
        if (reason == ForbidSpellbookReason.Alignment) {
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(AbilityTargetAlignment), nameof(AbilityTargetAlignment.IsTargetRestrictionPassed)), HarmonyPostfix]
    public static void AbilityTargetAlignment_IsTargetRestrictionPassed_Patch(ref bool __result) {
        __result = true;
    }
}
