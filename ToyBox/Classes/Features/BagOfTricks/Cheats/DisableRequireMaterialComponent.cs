using Kingmaker.UnitLogic.Abilities;

namespace ToyBox.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.DisableRequireMaterialComponent")]
public partial class DisableRequireMaterialComponent : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.DisableRequireMaterialComponent";
    public override ref bool IsEnabled => ref Settings.DisableRequireMaterialComponent;
    [LocalizedString("ToyBox_Features_BagOfTricks_DisableRequireMaterialComponent_NoMaterialComponentsText", "No Material Components")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DisableRequireMaterialComponent_AbilitiesNoLongerNeedAnyMaterial", "Abilities no longer need any material components which they would otherwise consume")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.RequireMaterialComponent), MethodType.Getter), HarmonyPostfix]
    public static void AbilityData_RequireMaterialComponent_Postfix(ref bool __result) {
        __result = false;
    }
}
