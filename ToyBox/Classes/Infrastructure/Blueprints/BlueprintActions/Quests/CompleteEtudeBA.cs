using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class CompleteEtudeBA : BlueprintActionFeature, IBlueprintAction<BlueprintEtude> {

    private bool CanExecute(BlueprintEtude blueprint) {
        return IsInGame() && Game.Instance.Player.EtudesSystem.EtudeIsStarted(blueprint);
    }
    private bool Execute(BlueprintEtude blueprint) {
        LogExecution(blueprint);
        Game.Instance.Player.EtudesSystem.MarkEtudeCompleted(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintEtude blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            UI.Button(CompleteText, () => {
                result = Execute(blueprint);
            });
        }
        return result;
    }

    public bool GetContext(out BlueprintEtude? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!);
        }
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteEtudeBA_Name", "Complete Etude")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteEtudeBA_Description", "Complete the specified BlueprintEtude. A failed Etude is often used to mark a failed quest state.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_CompleteEtudeBA_CompleteText", "Complete")]
    private static partial string CompleteText { get; }
}
