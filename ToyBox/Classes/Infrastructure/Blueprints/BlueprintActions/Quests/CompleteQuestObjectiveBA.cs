using Kingmaker;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Quests;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class CompleteQuestObjectiveBA : BlueprintActionFeature, IBlueprintAction<BlueprintQuestObjective> {

    private bool CanExecute(BlueprintQuestObjective blueprint) {
        return IsInGame() && (Game.Instance.Player.QuestBook.GetQuest(blueprint.Quest)?.TryGetObjective(blueprint)?.State ?? QuestObjectiveState.None) == QuestObjectiveState.Started;
    }
    private bool Execute(BlueprintQuestObjective blueprint) {
        LogExecution(blueprint);
        Game.Instance.Player.QuestBook.GiveObjective(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintQuestObjective blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            UI.Button(CompleteText, () => {
                result = Execute(blueprint);
            });
        }
        return result;
    }

    public bool GetContext(out BlueprintQuestObjective? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!);
        }
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteQuestObjectiveBA_Name", "Complete Quest Objective")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteQuestObjectiveBA_Description", "Completes the specified BlueprintQuestObjective.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteQuestObjectiveBA_CompleteText", "Complete")]
    private static partial string CompleteText { get; }
}
