using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.DisableArmourSpeedReductionFeature")]
public partial class DisableArmourSpeedReductionFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.DisableArmourSpeedReductionFeature";
    public override ref bool IsEnabled => ref Settings.ToggleDisableArmourSpeedReduction;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableArmourSpeedReductionFeature_Name", "Disable Armor Speed Reduction")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableArmourSpeedReductionFeature_Description", "Some armours and shields apply a modifier to speed. This feature removes said modifiers.")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(ItemEntityArmor), nameof(ItemEntityArmor.RecalculateStats)), HarmonyPostfix]
    public static void ItemEntityArmor_RecalculateStats_Patch(ItemEntityArmor __instance) {
        if (__instance.m_Modifiers != null) {
            __instance.m_Modifiers.ForEach(delegate (ModifiableValue.Modifier m) {
                var desc = m.ModDescriptor;
                if (m.AppliedTo == __instance.Wielder.Stats.Speed && (desc == ModifierDescriptor.Shield || desc == ModifierDescriptor.Armor)) {
                    m.Remove();
                }
            });
        }
    }
}
