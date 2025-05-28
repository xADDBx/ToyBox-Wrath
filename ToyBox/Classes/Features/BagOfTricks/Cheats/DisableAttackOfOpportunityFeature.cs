using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using ToyBox.Infrastructure;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.DisableAttackOfOpportunityFeature")]
public partial class DisableAttackOfOpportunityFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.DisableAttackOfOpportunityFeature";
    private bool m_IsEnabled;
    public override ref bool IsEnabled => ref m_IsEnabled;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableAttackOfOpportunityFeature_Name", "Disable Attacks Of Opportunity")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableAttackOfOpportunityFeature_Description", "Prevents the selected units from being attacked with an Attack of Opportunity")]
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
         m_IsEnabled = Settings.SelectionDisableAttackOfOpportunity != UnitSelectType.Off;
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
                if (UI.SelectionGrid(ref Settings.SelectionDisableAttackOfOpportunity, 6, (type) => type.GetLocalized(), AutoWidth())) {
                    if (Settings.SelectionDisableAttackOfOpportunity != UnitSelectType.Off) {
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
    [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.AttackOfOpportunity)), HarmonyPrefix]
    private static bool UnitCombatState_AttackOfOpportunity_Patch(UnitEntityData target) {
        if (ToyBoxUnitHelper.IsOfSelectedType(target, Settings.SelectionDisableAttackOfOpportunity)) {
            return false;
        }
        return true;
    }
}
