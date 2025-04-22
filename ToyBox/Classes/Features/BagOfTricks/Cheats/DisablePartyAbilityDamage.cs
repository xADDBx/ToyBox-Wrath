using Kingmaker.RuleSystem.Rules;
using ToyBox.Infrastructure;

namespace ToyBox.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.DisablePartyAbilityDamage")]
public partial class DisablePartyAbilityDamage : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.DisablePartyAbilityDamage";
    public override ref bool IsEnabled => ref Settings.DisablePartyAbilityDamage;
    [LocalizedString("ToyBox_Features_BagOfTricks_DisablePartyAbilityDamage_DisablePartyAbilityDamageText", "Disable Party Ability Damage")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DisablePartyAbilityDamage_PreventPartyMembersFromBeingAffe", "Prevent Party members from being affected by stat reducing attacks")] 
    public override partial string Description { get; }
    [HarmonyPatch(typeof(RuleDealStatDamage), nameof(RuleDealStatDamage.Immune), MethodType.Getter), HarmonyPostfix]
    public static void RuleDealStatDamage_Immune_Postfix(RuleDealStatDamage __instance , ref bool __result) {
        if (ToyBoxUnitHelper.IsPartyOrPet(__instance.Target)) {
            __result = true;
        }
    }
}
