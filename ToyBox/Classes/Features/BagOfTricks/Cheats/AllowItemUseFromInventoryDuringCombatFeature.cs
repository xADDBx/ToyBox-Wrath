using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.AllowItemUseFromInventoryDuringCombatFeature")]
public partial class AllowItemUseFromInventoryDuringCombatFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.AllowItemUseFromInventoryDuringCombatFeature";
    public override ref bool IsEnabled => ref Settings.ToggleAllowItemUseFromInventoryDuringCombat;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_AllowItemUseFromInventoryDuringCombatFeature_Name", "Allow inventory items in combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_AllowItemUseFromInventoryDuringCombatFeature_Description", "Makes it possible to use items in the inventory even in a fight")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(ItemEntity), nameof(ItemEntity.IsUsableFromInventory), MethodType.Getter), HarmonyPrefix]
    public static bool ItemEntity_IsUsableFromInventory_Patch(ItemEntity __instance, ref bool __result) {
        if (__instance.Blueprint is BlueprintItemEquipment item && item.Ability != null) {
            __result = true;
        } else {
            __result = false;
        }
        return false;
    }
}
