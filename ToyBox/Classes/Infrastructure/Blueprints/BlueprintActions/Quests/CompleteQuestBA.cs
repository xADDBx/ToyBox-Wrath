using Kingmaker;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Quests;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class CompleteQuestBA : BlueprintActionFeature, IBlueprintAction<BlueprintQuest> {

    private bool CanExecute(BlueprintQuest blueprint) {
        return IsInGame() && Game.Instance.Player.QuestBook.GetQuest(blueprint)?.State == QuestState.Started;
    }
    private bool Execute(BlueprintQuest blueprint) {
        LogExecution(blueprint);
        foreach (var objective in blueprint.Objectives) {
            Game.Instance.Player.QuestBook.CompleteObjective(objective);
        }
        return true;
    }
    public bool? OnGui(BlueprintQuest blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            UI.Button(CompleteText, () => {
                result = Execute(blueprint);
            });
        }
        return result;
    }
    public bool GetContext(out BlueprintQuest? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!);
        }
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteQuestBA_Name", "Complete Quest")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteQuestBA_Description", "Completes the specified BlueprintQuest by forcing each objective to complete.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteQuestBA_CompleteText", "Complete")]
    private static partial string CompleteText { get; }
}
