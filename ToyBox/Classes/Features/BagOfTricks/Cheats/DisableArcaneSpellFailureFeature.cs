using Kingmaker.RuleSystem.Rules.Abilities;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.DisableArcaneSpellFailureFeature")]
public partial class DisableArcaneSpellFailureFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.DisableArcaneSpellFailureFeature";
    public override ref bool IsEnabled => ref Settings.ToggleDisableArcaneSpellFailure;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableArcaneSpellFailureFeature_Name", "Disable Arcane Spell Failure")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableArcaneSpellFailureFeature_Description", "Sets the arcane spell failure chance on spells to 0")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(RuleCastSpell), nameof(RuleCastSpell.ArcaneSpellFailureChance), MethodType.Getter), HarmonyPostfix]
    public static void RuleCastSpell_ArcaneSpellFailureChance_Patch(ref int __result) {
        __result = 0;
    }
}
