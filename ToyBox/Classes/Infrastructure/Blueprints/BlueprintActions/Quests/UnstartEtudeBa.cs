using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class UnstartEtudeBA : BlueprintActionFeature, IBlueprintAction<BlueprintEtude> {

    private bool CanExecute(BlueprintEtude blueprint) {
        return IsInGame() && !Game.Instance.Player.EtudesSystem.EtudeIsNotStarted(blueprint);
    }
    private bool Execute(BlueprintEtude blueprint) {
        LogExecution(blueprint);
        Game.Instance.Player.EtudesSystem.UnstartEtude(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintEtude blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            var text = UnstartText;
            if (isFeatureSearch) {
                text = text.Cyan().Bold().SizePercent(115);
            }
            UI.Button(text, () => {
                result = Execute(blueprint);
            });
        } else if (isFeatureSearch) {
            if (IsInGame()) {
                UI.Label(EtudeIsNotStartedText.Red().Bold());
            } else {
                UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
            }
        }
        return result;
    }

    public bool GetContext(out BlueprintEtude? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!, true);
        }
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_UnstartEtudeBA_Name", "Unstart Etude")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_UnstartEtudeBA_Description", "Unstarts the specified BlueprintEtude.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_UnstartEtudeBA_UnstartText", "Unstart")]
    private static partial string UnstartText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_UnstartEtudeBA_EtudeIsNotStartedText", "Etude is not started or completed")]
    private static partial string EtudeIsNotStartedText { get; }
}
