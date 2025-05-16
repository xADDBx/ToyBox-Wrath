using Kingmaker.Crusade.GlobalMagic.SpellsManager;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.InstantGlobalCrusadeSpellsFeature")]
public partial class InstantGlobalCrusadeSpellsFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.InstantGlobalCrusadeSpellsFeature";
    public override ref bool IsEnabled => ref Settings.ToggleInstantGlobalCrusadeSpells;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_InstantGlobalCrusadeSpellsFeature_Name", "Instant Global Crusade Spells Cooldown")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_InstantGlobalCrusadeSpellsFeature_Description", "Restores crusade spells immediately after using them")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(SpellState), nameof(SpellState.WasUsed)), HarmonyPostfix]
    public static void SpellState_WasUsed_Patch(SpellState __instance) {
        __instance.RestoreImmediately();
    }
}
