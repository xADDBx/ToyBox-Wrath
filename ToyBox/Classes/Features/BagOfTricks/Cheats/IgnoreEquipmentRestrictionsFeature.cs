using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using Kingmaker.UnitLogic;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.IgnoreEquipmentRestrictionsFeature")]
public partial class IgnoreEquipmentRestrictionsFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.IgnoreEquipmentRestrictionsFeature";
    public override ref bool IsEnabled => ref Settings.ToggleIgnoreEquipmentRestrictions;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_IgnoreEquipmentRestrictionsFeature_Name", "Ignore Equipment Restrictions")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_IgnoreEquipmentRestrictionsFeature_Description", "Ignores restrictions when equipping weapons, shields, armour and other items")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(BlueprintItemEquipment), nameof(BlueprintItemEquipment.CanBeEquippedBy)), HarmonyPostfix]
    public static void BlueprintItemEquipment_CanBeEquippedBy_Patch(ref bool __result) {
        __result = true;
    }
    [HarmonyPatch(typeof(ItemEntityArmor), nameof(ItemEntityArmor.CanBeEquippedInternal)), HarmonyPostfix]
    public static void ItemEntityArmor_CanBeEquippedInternal_Patch(ItemEntityArmor __instance, UnitDescriptor owner, ref bool __result) {
        if (__instance.Blueprint is BlueprintItemEquipment equip) {
            __result = equip.CanBeEquippedBy(owner);
        }
    }
    [HarmonyPatch(typeof(ItemEntityShield), nameof(ItemEntityShield.CanBeEquippedInternal)), HarmonyPostfix]
    public static void ItemEntityShield_CanBeEquippedInternal_Patch(ItemEntityShield __instance, UnitDescriptor owner, ref bool __result) {
        if (__instance.Blueprint is BlueprintItemEquipment equip) {
            __result = equip.CanBeEquippedBy(owner);
        }
    }
    [HarmonyPatch(typeof(ItemEntityWeapon), nameof(ItemEntityWeapon.CanBeEquippedInternal)), HarmonyPostfix]
    public static void ItemEntityWeapon_CanBeEquippedInternal_Patch(ItemEntityWeapon __instance, UnitDescriptor owner, ref bool __result) {
        if (__instance.Blueprint is BlueprintItemEquipment equip) {
            __result = equip.CanBeEquippedBy(owner);
        }
    }
}
