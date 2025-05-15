using Kingmaker.UnitLogic.Abilities;
using ToyBox.Infrastructure;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.Cheats.InfiniteSpellCastsFeature")]
public partial class InfiniteSpellCastsFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.InfiniteSpellCastsFeature";
    public override ref bool IsEnabled => ref Settings.ToggleInfiniteSpellCasts;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_InfiniteSpellCastsFeature_Name", "Infinite Spell Casts")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_InfiniteSpellCastsFeature_Description", "Turns spell slot cost of spells to 0")]
    public override partial string Description { get; }

    [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.SpellSlotCost), MethodType.Getter), HarmonyPostfix]
    private static void AbilityData_SpellSlotCost_Patch(ref int __result, AbilityData __instance) {
        if (ToyBoxUnitHelper.IsPartyOrPet(__instance.Fact.Owner)) {
            __result = 0;
        }
    }
}
