using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.DisableFogOfWarFeature")]
public partial class DisableFogOfWarFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.DisableFogOfWarFeature";
    public override bool IsEnabled => Settings.DisableFoW;

    [LocalizedString("ToyBox_Features_BagOfTricks_DisableFogOfWarFeature_DisableFogOfWarText", "Disable Fog of War")]
    public override partial string Name { get; }

    [LocalizedString("ToyBox_Features_BagOfTricks_DisableFogOfWarFeature_DisableTheFogOfWarOnTheMapText", "Disable the fog of war on the map")]
    public override partial string Description { get; }
    public override void OnGui() {
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(Settings.DisableFoW, Name.Cyan(), GUILayout.ExpandWidth(false));
            if (newValue != Settings.DisableFoW) {
                Settings.DisableFoW = newValue;
                if (newValue) {
                    Initialize();
                } else {
                    Destroy();
                }
            }
            GUILayout.Space(10);
            GUILayout.Label(Description.Green(), GUILayout.ExpandWidth(false));
        }
    }
    [HarmonyPatch(typeof(FogOfWarArea), nameof(FogOfWarArea.RevealOnStart), MethodType.Getter), HarmonyPrefix]
    public static bool FogOfWarReveal_Prefix(ref bool __result) {
        __result = true; 
        return false;
    }
}
