using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.Cheats.DisableFogOfWarFeature")]
public partial class DisableFogOfWarFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.DisableFogOfWarFeature";
    public override ref bool IsEnabled => ref Settings.DisableFoW;

    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableFogOfWarFeature_DisableFogOfWarText", "Disable Fog of War")]
    public override partial string Name { get; }

    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableFogOfWarFeature_DisableTheFogOfWarOnTheMapText", "Disable the fog of war on the map")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(FogOfWarArea), nameof(FogOfWarArea.RevealOnStart), MethodType.Getter), HarmonyPostfix]
    public static void FogOfWarArea_RevealOnStart_Prefix(ref bool __result) {        __result = true;
    }
}
