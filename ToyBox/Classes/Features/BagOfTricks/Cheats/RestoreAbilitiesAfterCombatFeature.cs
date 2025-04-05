using Kingmaker;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.RestoreAbilitiesAfterCombatFeature")]
public partial class RestoreAbilitiesAfterCombatFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.RestoreAbilitiesAfterCombatFeature";
    public override bool IsEnabled => Settings.RestoreAbilitiesAfterCombat;

    [LocalizedString("ToyBox_Features_BagOfTricks_RestoreAbilitiesAfterCombatFeature_RestoreAbilitiesAfterCombatText", "Restore Abilities After Combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RestoreAbilitiesAfterCombatFeature_RestoresAllChargesOnAbilitiesAft", "Restores all charges on abilities after combat")]
    public override partial string Description { get; }
    public override void OnGui() {
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(Settings.RestoreAbilitiesAfterCombat, Name.Cyan(), GUILayout.ExpandWidth(false));
            if (newValue != Settings.RestoreAbilitiesAfterCombat) {
                Settings.RestoreAbilitiesAfterCombat = newValue;
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
                foreach (var resource in unit.Descriptor.Resources) {
                    unit.Descriptor.Resources.Restore(resource);
                }
                unit.Brain.RestoreAvailableActions();
            }
        }
    }
}
