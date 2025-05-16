using Kingmaker.RuleSystem.Rules.Abilities;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.DisableSpellFailureFeature")]
public partial class DisableSpellFailureFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.DisableSpellFailureFeature";
    public override ref bool IsEnabled => ref Settings.ToggleDisableSpellFailure;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableSpellFailureFeature_Name", "Disable Spell Failure")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableSpellFailureFeature_Description", "Sets the spell failure chance on spells to 0")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(RuleCastSpell), nameof(RuleCastSpell.SpellFailureChance), MethodType.Getter), HarmonyPostfix]
    private static void RuleCastSpell_SpellFailureChance_Patch(ref int __result) {
        __result = 0;
    }
}
