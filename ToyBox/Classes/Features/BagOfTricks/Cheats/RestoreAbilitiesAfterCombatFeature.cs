using Kingmaker;

namespace ToyBox.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.RestoreAbilitiesAfterCombatFeature")]
public partial class RestoreAbilitiesAfterCombatFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.RestoreAbilitiesAfterCombatFeature";
    public override ref bool IsEnabled => ref Settings.RestoreAbilitiesAfterCombat;

    [LocalizedString("ToyBox_Features_BagOfTricks_RestoreAbilitiesAfterCombatFeature_RestoreAbilitiesAfterCombatText", "Restore Abilities After Combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RestoreAbilitiesAfterCombatFeature_RestoresAllChargesOnAbilitiesAft", "Restores all charges on abilities after combat")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(GameHistoryLog), nameof(GameHistoryLog.HandlePartyCombatStateChanged)), HarmonyPostfix]
    public static void CombatStateChanged_Postfix(ref bool inCombat) {
        if (!inCombat) {
            foreach (var unit in Game.Instance.Player.Party) {
                foreach (var resource in unit.Descriptor.Resources) {
                    unit.Descriptor.Resources.Restore(resource);
                }
                unit.Brain.RestoreAvailableActions();
            }
        }
    }
}
