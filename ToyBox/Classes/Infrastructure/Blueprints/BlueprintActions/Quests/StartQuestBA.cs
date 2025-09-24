using Kingmaker;
using Kingmaker.Blueprints.Quests;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class StartQuestBA : BlueprintActionFeature, IBlueprintAction<BlueprintQuest> {

    private bool CanExecute(BlueprintQuest blueprint) {
        return IsInGame() && Game.Instance.Player.QuestBook.GetQuest(blueprint) == null;
    }
    private bool Execute(BlueprintQuest blueprint) {
        LogExecution(blueprint);
        Game.Instance.Player.QuestBook.GiveObjective(blueprint.Objectives.First());
        return true;
    }
    public bool? OnGui(BlueprintQuest blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            UI.Button(StartText, () => {
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
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_StartQuestBA_Name", "Start Quest")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_StartQuestBA_Description", "Start a quest by starting its first objective.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_StartQuestBA_StartText", "Start")]
    private static partial string StartText { get; }
}
