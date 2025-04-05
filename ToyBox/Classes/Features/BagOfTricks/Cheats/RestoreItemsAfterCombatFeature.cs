using Kingmaker;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.RestoreItemsAfterCombatFeature")]
public partial class RestoreItemsAfterCombatFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.RestoreItemsAfterCombatFeature";
    public override bool IsEnabled => Settings.RestoreItemsAfterCombat;

    [LocalizedString("ToyBox_Features_BagOfTricks_RestoreItemsAfterCombatFeature_RestoreItemsAfterCombatText", "Restore Items After Combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RestoreItemsAfterCombatFeature_RestoreItemChargesAfterCombatTex", "Restore item charges after combat")]
    public override partial string Description { get; }
    public override void OnGui() {
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(Settings.RestoreItemsAfterCombat, Name.Cyan(), GUILayout.ExpandWidth(false));
            if (newValue != Settings.RestoreItemsAfterCombat) {
                Settings.RestoreItemsAfterCombat = newValue;
                if (newValue) {
                    Initialize();
                } else {
                    Destroy();
                }
            }
            GUILayout.Space(10);
            GUILayout.Label(Description.Green(), GUILayout.ExpandWidth(false));
        }
    }
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
