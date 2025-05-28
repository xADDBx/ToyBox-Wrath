using Kingmaker.AreaLogic.Etudes;

namespace ToyBox.Features.BagOfTricks.QualityOfLife;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.QualityOfLife.DisableTricksterMythicMusicFeature")]
public partial class DisableTricksterMythicMusicFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.QualityOfLife.DisableTricksterMythicMusicFeature";
    public override ref bool IsEnabled => ref Settings.ToggleDisableTricksterMythicMusic;
    [LocalizedString("ToyBox_Features_BagOfTricks_QualityOfLife_DisableTricksterMythicMusicFeature_Name", "Disable Trickster Mythic Music in Drezen")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_QualityOfLife_DisableTricksterMythicMusicFeature_Description", "Prevents the Trickster Theme from running in Drezen")]
    public override partial string Description { get; }
    private const string TricksterCouncil_Council5_2_Music_Etude = "61aa5a32f2934c9189d31f74759ea8de";
    private const string Trickster_MusicState_Drezen_Etude = "b6eaccea5fa954145a4a9d74fdbf7c62";
    [HarmonyPatch(typeof(EtudeBracketMusic), nameof(EtudeBracketMusic.OnEnter)), HarmonyPrefix]
    private static bool EtudeBracketMusic_OnEnter_Patch(EtudeBracketMusic __instance) {
        return __instance.OwnerBlueprint.AssetGuid != TricksterCouncil_Council5_2_Music_Etude;
    }
    [HarmonyPatch(typeof(EtudeBracketMusic), nameof(EtudeBracketMusic.OnResume)), HarmonyPrefix]
    private static bool EtudeBracketMusic_OnResume_Patch(EtudeBracketMusic __instance) {
        return __instance.OwnerBlueprint.AssetGuid != TricksterCouncil_Council5_2_Music_Etude;
    }
    [HarmonyPatch(typeof(EtudeBracketAudioEvents), nameof(EtudeBracketAudioEvents.OnEnter)), HarmonyPrefix]
    private static bool EtudeBracketAudioEvents_OnEnter_Patch(EtudeBracketAudioEvents __instance) {
        return __instance.OwnerBlueprint.AssetGuid != Trickster_MusicState_Drezen_Etude;
    }
    [HarmonyPatch(typeof(EtudeBracketAudioEvents), nameof(EtudeBracketAudioEvents.OnResume)), HarmonyPrefix]
    private static bool EtudeBracketAudioEvents_OnResume_Patch(EtudeBracketAudioEvents __instance) {
        return __instance.OwnerBlueprint.AssetGuid != Trickster_MusicState_Drezen_Etude;
    }
}
