using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class StartEtudeBA : BlueprintActionFeature, IBlueprintAction<BlueprintEtude> {
    private bool CanExecute(BlueprintEtude blueprint) {
        return IsInGame() && Game.Instance.Player.EtudesSystem.EtudeIsNotStarted(blueprint);
    }
    private bool Execute(BlueprintEtude blueprint) {
        LogExecution(blueprint);
        Game.Instance.Player.EtudesSystem.StartEtude(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintEtude blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            UI.Button(StartText, () => {
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
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_StartEtudeBA_Name", "Start Etude")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_StartEtudeBA_Description", "Starts the specified BlueprintEtude.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_StartEtudeBA_StartText", "Start")]
    private static partial string StartText { get; }
}
