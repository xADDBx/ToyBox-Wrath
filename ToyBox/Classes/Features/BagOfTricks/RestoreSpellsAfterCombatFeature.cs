using Kingmaker;
using UnityEngine;

namespace ToyBox.Classes.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Classes.Features.BagOfTricks.RestoreSpellsAfterCombatFeature")]
public partial class RestoreSpellsAfterCombatFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Classes.Features.BagOfTricks.RestoreSpellsAfterCombatFeature";
    public override bool IsEnabled => Settings.RestoreSpellsAfterCombat;

    [LocalizedString("ToyBox_Classes_Features_BagOfTricks_RestoreSpellsAfterCombatFeature_RestoreSpellsAfterCombatText", "Restore Spells After Combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Classes_Features_BagOfTricks_RestoreSpellsAfterCombatFeature_RestoreSpellChargesAfterCombatTe", "Restore spell charges after combat")]
    public override partial string Description { get; }
    public override void OnGui() {
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(Settings.RestoreSpellsAfterCombat, Name.Cyan(), GUILayout.ExpandWidth(false));
            if (newValue != Settings.RestoreSpellsAfterCombat) {
                Settings.RestoreSpellsAfterCombat = newValue;
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
                foreach (var spellbook in unit.Descriptor.Spellbooks)
                    spellbook.Rest();
            }
        }
    }
}
