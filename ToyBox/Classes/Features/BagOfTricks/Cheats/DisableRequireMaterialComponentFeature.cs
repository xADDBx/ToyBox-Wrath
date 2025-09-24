using Kingmaker.UnitLogic.Abilities;

namespace ToyBox.Features.BagOfTricks.Cheats;

[NeedsTesting]
[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.DisableRequireMaterialComponentFeature")]
public partial class DisableRequireMaterialComponentFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.DisableRequireMaterialComponentFeature";
    public override ref bool IsEnabled => ref Settings.DisableRequireMaterialComponent;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableRequireMaterialComponent_NoMaterialComponentsText", "No Material Components")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableRequireMaterialComponent_AbilitiesNoLongerNeedAnyMaterial", "Abilities no longer need any material components which they would otherwise consume")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.RequireMaterialComponent), MethodType.Getter), HarmonyPostfix]
    private static void AbilityData_RequireMaterialComponent_Postfix(ref bool __result) {
        __result = false;
    }
}
