using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Utility;

namespace ToyBox.Features.BagOfTricks.Cheats;

[NeedsTesting]
[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.NoFriendlyFireAoEFeature")]
public partial class NoFriendlyFireAoEFeature : FeatureWithPatch {
#warning https://discord.com/channels/645948717400064030/815735034514112512/1377588591567573152 (not working)
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.NoFriendlyFireAoEFeature";
    public override ref bool IsEnabled => ref Settings.ToggleNoFriendlyFireAoEFeature;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_NoFriendlyFireAoEFeature_Name", "No Friendly Fire On AoEs")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_NoFriendlyFireAoEFeature_Description", "Tries to remove teammates as targets in harmful AoEs. Also sets damage to zero and lets Statchecks, Skillchecks and Saving throws succeeds if both attacker and attackee are friendly.")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(AbilityTargetsAround), nameof(AbilityTargetsAround.Select)), HarmonyPostfix]
    private static void AbilityTargetsAround_Select_Patch(ref IEnumerable<TargetWrapper> __result, AbilityExecutionContext context) {
        if (ToyBoxUnitHelper.IsPartyOrPet(context.Caster) && context.AbilityBlueprint.EffectOnAlly == AbilityEffectOnUnit.Harmful) {
            __result = __result.Where(tw => !tw.Unit.IsAlly(context.Caster));
        }
    }
    [HarmonyPatch(typeof(RuleDealDamage), nameof(RuleDealDamage.Result), MethodType.Getter), HarmonyPostfix]
    private static void RuleDealDamage_ApplyDifficultyModifiers_Patch(ref int __result, RuleDealDamage __instance) {
        if (ShouldOverride(__instance)) {
            __result = 0;
        }
    }
    [HarmonyPatch(typeof(RuleSkillCheck), nameof(RuleSkillCheck.IsSuccessRoll), [typeof(int), typeof(int)]), HarmonyPostfix]
    private static void RuleSkillCheck_IsSuccessRoll_Patch(ref bool __result, RuleSkillCheck __instance) {
        if (ShouldOverride(__instance)) {
            __result = true;
        }
    }
    [HarmonyPatch(typeof(RulePartyStatCheck), nameof(RulePartyStatCheck.Success), MethodType.Getter), HarmonyPostfix]
    private static void RulePartyStatCheck_Success_Patch(ref bool __result, RulePartyStatCheck __instance) {
        if (ShouldOverride(__instance)) {
            __result = true;
        }
    }
    [HarmonyPatch(typeof(RuleSavingThrow), nameof(RuleSavingThrow.IsPassed), MethodType.Getter), HarmonyPostfix]
    internal static void RuleSavingThrow_IsPassed_Patch(ref bool __result, RuleSavingThrow __instance) {
        if (ShouldOverride(__instance)) {
            __result = true;
        }
    }
    private static bool ShouldOverride(RulebookEvent __instance) {
        if (__instance.Reason?.Ability?.Blueprint is BlueprintAbility blueprintAbility 
            && ToyBoxUnitHelper.IsPartyOrPet(__instance.Reason.Caster)
            && ToyBoxUnitHelper.IsPartyOrPet(__instance.GetRuleTarget() ?? __instance.Initiator)
            && ((blueprintAbility.EffectOnAlly == AbilityEffectOnUnit.Harmful) || (blueprintAbility.EffectOnEnemy == AbilityEffectOnUnit.Harmful))) {
            return true;
        }
        return false;
    }
}
