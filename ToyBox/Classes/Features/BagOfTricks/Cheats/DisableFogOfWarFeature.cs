using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar;

namespace ToyBox.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.DisableFogOfWarFeature")]
public partial class DisableFogOfWarFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.DisableFogOfWarFeature";
    public override ref bool IsEnabled => ref Settings.DisableFoW;

    [LocalizedString("ToyBox_Features_BagOfTricks_DisableFogOfWarFeature_DisableFogOfWarText", "Disable Fog of War")]
    public override partial string Name { get; }

    [LocalizedString("ToyBox_Features_BagOfTricks_DisableFogOfWarFeature_DisableTheFogOfWarOnTheMapText", "Disable the fog of war on the map")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(FogOfWarArea), nameof(FogOfWarArea.RevealOnStart), MethodType.Getter), HarmonyPrefix]
    public static bool FogOfWarReveal_Prefix(ref bool __result) {
        __result = true; 
        return false;
    }
}
