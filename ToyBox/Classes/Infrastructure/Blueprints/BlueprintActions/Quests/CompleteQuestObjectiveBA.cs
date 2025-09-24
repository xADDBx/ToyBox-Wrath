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
        Game.Instance.Player.QuestBook.CompleteObjective(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintQuestObjective blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            var text = CompleteText;
            if (isFeatureSearch) {
                text = text.Cyan().Bold().SizePercent(115);
            }
            UI.Button(text, () => {
                result = Execute(blueprint);
            });
        } else if (isFeatureSearch) {
            if (IsInGame()) {
                UI.Label(QuestObjectiveIsNotStartedText.Red().Bold());
            } else {
                UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
            }
        }
        return result;
    }

    public bool GetContext(out BlueprintQuestObjective? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!, true);
        }
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteQuestObjectiveBA_Name", "Complete Quest Objective")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteQuestObjectiveBA_Description", "Completes the specified BlueprintQuestObjective.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteQuestObjectiveBA_CompleteText", "Complete")]
    private static partial string CompleteText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteQuestObjectiveBA_QuestObjectiveIsNotStartedText", "Quest objective is not started")]
    private static partial string QuestObjectiveIsNotStartedText { get; }
}
