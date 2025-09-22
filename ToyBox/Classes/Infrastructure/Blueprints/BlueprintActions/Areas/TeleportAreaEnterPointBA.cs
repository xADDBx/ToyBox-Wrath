using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Persistence;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class TeleportAreaEnterPointBA : IBlueprintAction<BlueprintAreaEnterPoint> {
    static TeleportAreaEnterPointBA() {
        BlueprintActions.RegisterAction((IBlueprintAction<SimpleBlueprint>)new TeleportAreaEnterPointBA());
    }
    private bool CanExecute(BlueprintAreaEnterPoint blueprint, params object[] parameter) {
        return IsInGame();
    }
    private bool Execute(BlueprintAreaEnterPoint blueprint, params object[] parameter) {
        ((IBlueprintAction<SimpleBlueprint>)this).LogBPAction(blueprint, parameter);
        GameHelper.EnterToArea(blueprint, AutoSaveMode.None);
        return true;
    }
    public bool? OnGui(BlueprintAreaEnterPoint blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(TeleportText, () => {
                result = Execute(blueprint, parameter);
            });
        }
        return result;
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportAreaEnterPointBA_TeleportText", "Teleport")]
    private static partial string TeleportText { get; }
}
