using Kingmaker;
using Kingmaker.Armies;
using Kingmaker.Armies.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class RemoveLeaderSkillBA : BlueprintActionFeature, IBlueprintAction<BlueprintLeaderSkill> {
    private bool CanExecute(BlueprintLeaderSkill blueprint, ArmyLeader? leader, params object[] parameter) {
        return IsInGame() && leader != null && leader.Skills.Contains(blueprint);
    }
    private bool Execute(BlueprintLeaderSkill blueprint, ArmyLeader leader, params object[] parameter) {
        LogExecution(blueprint, leader, parameter);
        leader.RemoveSkill(blueprint);
        return true;
    }
    private bool GetLeader(out ArmyLeader? leader) {
        leader = Game.Instance?.GlobalMapController?.SelectedArmy?.Data?.Leader;
        return leader != null;
    }
    public bool? OnGui(BlueprintLeaderSkill blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        GetLeader(out var leader);
        if (CanExecute(blueprint, leader, parameter)) {
            UI.Button(StyleActionString(RemoveText, isFeatureSearch), () => {
                result = Execute(blueprint, leader!, parameter);
            });
        } else if (isFeatureSearch) {
            if (IsInGame()) {
                if (leader != null) {
                    UI.Label(LeaderAlreadyHasTheSpecifiedSkil.Red().Bold());
                } else {
                    UI.Label(SelectAnArmyWithAValudLeaderFirs.Red().Bold());
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

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveLeaderSkillBA_RemoveText", "Remove")]
    private static partial string RemoveText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveLeaderSkillBA_Name", "Remove Leader Skill")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveLeaderSkillBA_Description", "Removes the specified BlueprintLeaderSkill.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveLeaderSkillBA_LeaderAlreadyHasTheSpecifiedSkil", "Leader does not have the specified skill")]
    private static partial string LeaderAlreadyHasTheSpecifiedSkil { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveLeaderSkillBA_SelectAnArmyWithAValudLeaderFirs", "Select an army with a valud Leader first")]
    private static partial string SelectAnArmyWithAValudLeaderFirs { get; }
}
