using Kingmaker.RuleSystem.Rules;
using ToyBox.Infrastructure;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.Cheats.DisableNegativePartyLevels")]
public partial class DisableNegativePartyLevels : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.DisableNegativePartyLevels";
    public override ref bool IsEnabled => ref Settings.DisableNegativePartyLevels;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableNegativePartyLevels_DisablePartyNegativeLevelsText", "Disable Party Negative Levels")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableNegativePartyLevels_PreventPartyMembersFromBeingAffe", "Prevent Party members from being affected by energy drain")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(RuleDrainEnergy), nameof(RuleDrainEnergy.TargetIsImmune), MethodType.Getter), HarmonyPostfix]
    public static void RuleDrainEnergy_TargetIsImmune_Postfix(RuleDrainEnergy __instance , ref bool __result) {
        if (ToyBoxUnitHelper.IsPartyOrPet(__instance.Target)) {
            __result = true;
        }
    }
}
