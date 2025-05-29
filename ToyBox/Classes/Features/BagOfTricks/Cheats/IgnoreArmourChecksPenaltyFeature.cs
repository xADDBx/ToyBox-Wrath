using Kingmaker.Blueprints.Items.Armors;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.IgnoreArmourChecksPenaltyFeature")]
public partial class IgnoreArmourChecksPenaltyFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.IgnoreArmourChecksPenaltyFeature";
    public override ref bool IsEnabled => ref Settings.ToggleIgnoreArmourChecksPenalty;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_IgnoreArmourChecksPenaltyFeature_Name", "Disable Armor Checks Penalty")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_IgnoreArmourChecksPenaltyFeature_Description", "Some armour has a penalty for Mobility, Athletics, Stealth and Thievery skill checks. This feature sets to penalty to 0.")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(BlueprintItemArmor), nameof(BlueprintItemArmor.ArmorChecksPenalty), MethodType.Getter), HarmonyPostfix]
    private static void BlueprintItemArmor_ArmorChecksPenalty_Patch(ref int __result) {
        __result = 0;
    }
}
