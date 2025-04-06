using Kingmaker;

namespace ToyBox.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.RestoreItemsAfterCombatFeature")]
public partial class RestoreItemsAfterCombatFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.RestoreItemsAfterCombatFeature";
    public override ref bool IsEnabled => ref Settings.RestoreItemsAfterCombat;

    [LocalizedString("ToyBox_Features_BagOfTricks_RestoreItemsAfterCombatFeature_RestoreItemsAfterCombatText", "Restore Items After Combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RestoreItemsAfterCombatFeature_RestoreItemChargesAfterCombatTex", "Restore item charges after combat")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(GameHistoryLog), nameof(GameHistoryLog.HandlePartyCombatStateChanged)), HarmonyPostfix]
    public static void CombatStateChanged_Postfix(ref bool inCombat) {
        if (!inCombat) {
            foreach (var unit in Game.Instance.Player.Party) {
                foreach (var item in unit.Inventory.Items)
                    item.RestoreCharges();
            }
        }
    }
}
