using Kingmaker;
using Kingmaker.Cheats;
using UnityEngine;

namespace ToyBox.Classes.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Classes.Features.BagOfTricks.RestAfterCombatFeature")]
public partial class RestAfterCombatFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Classes.Features.BagOfTricks.RestAfterCombatFeature";
    public override bool IsEnabled => Settings.RestAfterCombat;

    [LocalizedString("ToyBox_Classes_Features_BagOfTricks_RestAfterCombatFeature_RestPartyInstantlyAfterCombatTex", "Rest Party Instantly After Combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Classes_Features_BagOfTricks_RestAfterCombatFeature_RestAllPartyMembersInstantlyAfte", "Rest all party members instantly after combat")]
    public override partial string Description { get; }
    public override void OnGui() {
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(Settings.RestAfterCombat, Name.Cyan(), GUILayout.ExpandWidth(false));
            if (newValue != Settings.RestAfterCombat) {
                Settings.RestAfterCombat = newValue;
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
            CheatsCombat.RestAll();
        }
    }
}
