using Kingmaker.RuleSystem.Rules;
using ToyBox.Infrastructure;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.DisableNegativePartyLevelsFeature")]
public partial class DisableNegativePartyLevelsFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.DisableNegativePartyLevelsFeature";
    public override ref bool IsEnabled => ref Settings.DisableNegativePartyLevels;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableNegativePartyLevels_DisablePartyNegativeLevelsText", "Stop party levels from being reduced")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableNegativePartyLevels_PreventPartyMembersFromBeingAffe", "Prevent Party members from losing levels due to energy drain")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(RuleDrainEnergy), nameof(RuleDrainEnergy.TargetIsImmune), MethodType.Getter), HarmonyPostfix]
    private static void RuleDrainEnergy_TargetIsImmune_Postfix(RuleDrainEnergy __instance, ref bool __result) {
        if (ToyBoxUnitHelper.IsPartyOrPet(__instance.Target)) {
            __result = true;
        }
    }
}
