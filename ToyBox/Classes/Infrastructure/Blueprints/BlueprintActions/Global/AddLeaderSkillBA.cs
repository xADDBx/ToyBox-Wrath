using Kingmaker;
using Kingmaker.Armies;
using Kingmaker.Armies.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class AddLeaderSkillBA : BlueprintActionFeature, IBlueprintAction<BlueprintLeaderSkill> {
    public bool CanExecute(BlueprintLeaderSkill blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is ArmyLeader leader) {
            return IsInGame() && !leader.Skills.Contains(blueprint);
        } else {
            return false;
        }
    }
    private bool Execute(BlueprintLeaderSkill blueprint, ArmyLeader leader, params object[] parameter) {
        LogExecution(blueprint, leader, parameter);
        leader.AddSkill(blueprint, true);
        return true;
    }
    private bool GetLeader(out ArmyLeader? leader) {
        leader = Game.Instance?.GlobalMapController?.SelectedArmy?.Data?.Leader;
        return leader != null;
    }
    public bool? OnGui(BlueprintLeaderSkill blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (GetLeader(out var leader) && CanExecute(blueprint, leader!, parameter)) {
            UI.Button(StyleActionString(AddText, isFeatureSearch), () => {
                result = Execute(blueprint, leader!, parameter);
            });
        } else if (isFeatureSearch) {
            if (IsInGame()) {
                if (leader != null) {
                    UI.Label(LeaderAlreadyHasTheSpecifiedSkil.Red().Bold());
                } else {
                    UI.Label(SelectAnArmyWithAValidLeaderFirs.Red().Bold());
                }
            } else {
                UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
            }
        }
        return result;
    }
    public bool GetContext(out BlueprintLeaderSkill? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!, true);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddLeaderSkillBA_AddText", "Add")]
    private static partial string AddText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddLeaderSkillBA_Name", "Add Leader Skill")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddLeaderSkillBA_Description", "Adds the specified BlueprintLeaderSkill.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddLeaderSkillBA_LeaderAlreadyHasTheSpecifiedSkil", "Leader already has the specified skill")]
    private static partial string LeaderAlreadyHasTheSpecifiedSkil { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddLeaderSkillBA_SelectAnArmyWithAValidLeaderFirs", "Select an army with a valid Leader first")]
    private static partial string SelectAnArmyWithAValidLeaderFirs { get; }
}
