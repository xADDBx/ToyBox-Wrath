using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints.Items.Components;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.SpontaneousCasterCopyScrollFeature")]
public partial class SpontaneousCasterCopyScrollFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.SpontaneousCasterCopyScrollFeature";
    public override ref bool IsEnabled => ref Settings.SpontaneousCasterCanCopyScrolls;

    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_SpontaneousCasterCopyScrollFeature_CanCopyScrollsText", "Spontaneous Caster Scroll Copy")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_SpontaneousCasterCopyScrollFeature_AllowSpontaneousCastersToCopyScr", "Allow spontaneous casters to copy scrolls into their spell books")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(CopyScroll), nameof(CopyScroll.CanCopySpell), [typeof(BlueprintAbility), typeof(Spellbook)]), HarmonyPostfix]
    private static void CopyScrolls_Postfix(BlueprintAbility spell, Spellbook spellbook, ref bool __result) {
        if (spellbook.IsKnown(spell)) {
            __result = false;
            return;
        }
        var spellListContainsSpell = spellbook.Blueprint.SpellList.Contains(spell);

        if (spellbook.Blueprint.Spontaneous && spellListContainsSpell) {
            __result = true;
            return;
        }

        __result = spellbook.Blueprint.CanCopyScrolls && spellListContainsSpell;
    }
}
