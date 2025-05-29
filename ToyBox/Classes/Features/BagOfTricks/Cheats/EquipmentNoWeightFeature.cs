using Kingmaker;
using Kingmaker.Items;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.EquipmentNoWeightFeature")]
public partial class EquipmentNoWeightFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.EquipmentNoWeightFeature";
    public override ref bool IsEnabled => ref Settings.ToggleEquipmentNoWeight;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_EquipmentNoWeightFeature_Name", "Equipment no weight")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_EquipmentNoWeightFeature_Description", "Makes things have no weight in both the collective and the individual inventories")]
    public override partial string Description { get; }
    public override void Initialize() {
        base.Initialize();
        Main.ScheduleForMainThread(() => {
            Game.Instance?.Player?.Inventory?.UpdateWeight();
        });
    }
    public override void Destroy() {
        base.Destroy();
        Main.ScheduleForMainThread(() => {
            Game.Instance?.Player?.Inventory?.UpdateWeight();
        });
    }
    [HarmonyPatch(typeof(ItemsCollection), nameof(ItemsCollection.DeltaWeight)), HarmonyPrefix]
    private static bool ItemsCollection_DeltaWeight_Patch(ItemsCollection __instance) {
        if (__instance.IsPlayerInventory) {
            __instance.Weight = 0;
        }
        return false;
    }
    [HarmonyPatch(typeof(ItemsCollection), nameof(ItemsCollection.UpdateWeight)), HarmonyPrefix]
    private static bool ItemsCollection_UpdateWeight_Patch(ItemsCollection __instance) {
        if (__instance.IsPlayerInventory) {
            __instance.Weight = 0;
        }
        return false;
    }
    [HarmonyPatch(typeof(UnitBody), nameof(UnitBody.EquipmentWeight), MethodType.Getter), HarmonyPrefix]
    public static bool UnitBody_EquipmentWeight_Patch(ref float __result) {
        __result = 0f;
        return false;
    }
    [HarmonyPatch(typeof(UnitBody), nameof(UnitBody.EquipmentWeightAfterBuff), MethodType.Getter), HarmonyPrefix]
    public static bool UnitBody_EquipmentWeightAfterBuff_Patch(ref float __result) {
        __result = 0f;
        return false;
    }
}
