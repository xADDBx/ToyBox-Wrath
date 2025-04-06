using Kingmaker;
using Kingmaker.Cheats;

namespace ToyBox.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.RestAfterCombatFeature")]
public partial class RestAfterCombatFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.RestAfterCombatFeature";
    public override ref bool IsEnabled => ref Settings.RestAfterCombat;

    [LocalizedString("ToyBox_Features_BagOfTricks_RestAfterCombatFeature_RestPartyInstantlyAfterCombatTex", "Rest Party Instantly After Combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RestAfterCombatFeature_RestAllPartyMembersInstantlyAfte", "Rest all party members instantly after combat")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(GameHistoryLog), nameof(GameHistoryLog.HandlePartyCombatStateChanged)), HarmonyPostfix]
    public static void CombatStateChanged_Postfix(ref bool inCombat) {
        if (!inCombat) {
            CheatsCombat.RestAll();
        }
    }
}
