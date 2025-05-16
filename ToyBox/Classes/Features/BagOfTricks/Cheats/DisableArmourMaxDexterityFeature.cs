using Kingmaker.Blueprints.Items.Armors;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.DisableArmourMaxDexterityFeature")]
public partial class DisableArmourMaxDexterityFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.DisableArmourMaxDexterityFeature";
    public override ref bool IsEnabled => ref Settings.ToggleDisableArmourMaxDexterity;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableArmourMaxDexterityFeature_Name", "Disable Armor Max Dexterity")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableArmourMaxDexterityFeature_Description", "Some armours have a limit to their dexterity bonus. This feature disables said limit for every armour.")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(BlueprintArmorType), nameof(BlueprintArmorType.HasDexterityBonusLimit), MethodType.Getter), HarmonyPostfix]
    public static void get_BlueprintArmorType_HasDexterityBonusLimit_Patch(ref bool __result) {
        __result = false;
    }
}
