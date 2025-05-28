using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using ToyBox.Infrastructure;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.AllowMovingThroughUnitsFeature")]
public partial class AllowMovingThroughUnitsFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.AllowMovingThroughUnitsFeature";
    private bool m_IsEnabled;
    public override ref bool IsEnabled => ref m_IsEnabled;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_AllowMovingThroughUnitsFeature_Name", "Can Move Through")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_AllowMovingThroughUnitsFeature_Description", "This allows characters you control to move through the selected category of units during combat")]
    public override partial string Description { get; }
    public override void Initialize() {
        UpdateEnabled();
        base.Initialize();
    }
    public override void Destroy() {
        UpdateEnabled();
        base.Destroy();
    }
    private void UpdateEnabled() {
        m_IsEnabled = Settings.SelectionAllowMovingThroughUnits != UnitSelectType.Off;
    }
    public override void OnGui() {
        using (VerticalScope()) {
            using (HorizontalScope()) {
                Space(27);
                UI.Label(Name.Cyan());
                Space(10);
                UI.Label(Description.Green());
            }
            using (HorizontalScope()) {
                Space(150);
                if (UI.SelectionGrid(ref Settings.SelectionAllowMovingThroughUnits, 6, (type) => type.GetLocalized(), AutoWidth())) {
                    if (Settings.SelectionAllowMovingThroughUnits != UnitSelectType.Off) {
                        if (!m_IsEnabled) {
                            Initialize();
                        }
                    } else {
                        if (m_IsEnabled) {
                            Destroy();
                        }
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(UnitMovementAgent), nameof(UnitMovementAgent.AvoidanceDisabled), MethodType.Getter), HarmonyPostfix]
    private static void UnitMovementAgent_AvoidanceDisabled_Patch(UnitMovementAgent __instance, ref bool __result) {
        if (ToyBoxUnitHelper.IsOfSelectedType(__instance.Unit?.EntityData, Settings.SelectionAllowMovingThroughUnits)) {
            __result = true;
        }
    }

    // Narria: Forbid moving through non selected entity type
    [HarmonyPatch(typeof(UnitMovementAgent), nameof(UnitMovementAgent.IsSoftObstacle), typeof(UnitMovementAgent)), HarmonyPrefix]
    private static bool UnitMovementAgent_IsSoftObstacle_Patch(UnitMovementAgent __instance, ref bool __result) {
        if (!ToyBoxUnitHelper.IsOfSelectedType(__instance.Unit?.EntityData, Settings.SelectionAllowMovingThroughUnits)) {
            // This duplicates the logic in the original logic for IsSoftObstacle.
            // If we are not in combat mode and it is not in our allow movement through category then it is a soft obstacle
            __result = !__instance.CombatMode;
            return false;
        }
        return true;
    }
}
