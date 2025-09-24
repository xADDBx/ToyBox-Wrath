using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using Kingmaker.UnitLogic;

namespace ToyBox.Features.BagOfTricks.Cheats;

[NeedsTesting]
[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.InfiniteItemChargesFeature")]
public partial class InfiniteItemChargesFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.InfiniteItemChargesFeature";
    public override ref bool IsEnabled => ref Settings.ToggleInfiniteItemCharges;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_InfiniteItemChargesFeature_Name", "Infinite Charges On Items")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_InfiniteItemChargesFeature_Description", "Using an item no longer spends any charge")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(ItemEntity), nameof(ItemEntity.SpendCharges), [typeof(UnitDescriptor)]), HarmonyPrefix]
    private static bool ItemEntity_SpendCharges_Patch(ref bool __result, ItemEntity __instance, UnitDescriptor user) {
        if (ToyBoxUnitHelper.IsPartyOrPet(user)) {
            if (__instance.Blueprint is BlueprintItemEquipment equip) {
                __result = equip.GainAbility;
                return false;
            }
        }
        return true;
    }
}
