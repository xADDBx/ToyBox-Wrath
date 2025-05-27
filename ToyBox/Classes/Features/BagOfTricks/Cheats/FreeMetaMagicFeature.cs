using Kingmaker.UnitLogic.Abilities;
using System.Reflection.Emit;
using System.Reflection;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.FreeMetaMagicFeature")]
public partial class FreeMetaMagicFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.FreeMetaMagicFeature";
    public override ref bool IsEnabled => ref Settings.ToggleFreeMetaMagic;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_FreeMetaMagicFeature_Name", "Free Meta-Magic")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_FreeMetaMagicFeature_Description", "Metamagic no longer increases the effective spell level or, in case of spontaneous casters, casting time.")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.RequireFullRoundAction), MethodType.Getter), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> get_AbilityData_RequireFullRoundAction_Patch(IEnumerable<CodeInstruction> instructions) {
        MethodInfo isSpontaneousMethod = AccessTools.PropertyGetter(typeof(AbilityData), nameof(AbilityData.IsSpontaneous));
        foreach (CodeInstruction c in instructions) {
            if (c.Calls(isSpontaneousMethod)) {
                yield return new(OpCodes.Pop);
                yield return new(OpCodes.Ldc_I4_0);
            } else {
                yield return c;
            }
        }
    }
    [HarmonyPatch(typeof(MetamagicHelper), nameof(MetamagicHelper.DefaultCost))]
    private static void Postfix(ref int __result) {
        __result = Math.Min(0, __result);
    }
}
