using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using ToyBox.Infrastructure;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.DisableAttackOfOpportunityFeature")]
public partial class DisableAttackOfOpportunityFeature : FeatureWIthUnitSelectTypeGrid {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.DisableAttackOfOpportunityFeature";
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableAttackOfOpportunityFeature_Name", "Disable Attacks Of Opportunity")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableAttackOfOpportunityFeature_Description", "Prevents the selected units from being attacked with an Attack of Opportunity")]
    public override partial string Description { get; }
    public override ref UnitSelectType SelectSetting => ref Settings.SelectionDisableAttackOfOpportunity;
    [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.AttackOfOpportunity)), HarmonyPrefix]
    private static bool UnitCombatState_AttackOfOpportunity_Patch(UnitEntityData target) {
        if (ToyBoxUnitHelper.IsOfSelectedType(target, Settings.SelectionDisableAttackOfOpportunity)) {
            return false;
        }
        return true;
    }
}
