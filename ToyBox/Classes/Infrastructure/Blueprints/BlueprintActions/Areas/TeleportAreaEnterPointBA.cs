using Kingmaker.Blueprints.Area;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Persistence;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class TeleportAreaEnterPointBA : BlueprintActionFeature, IBlueprintAction<BlueprintAreaEnterPoint> {
    private bool CanExecute(BlueprintAreaEnterPoint blueprint, params object[] parameter) {
        return IsInGame();
    }
    private bool Execute(BlueprintAreaEnterPoint blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
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
    public bool GetContext(out BlueprintAreaEnterPoint? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportAreaEnterPointBA_TeleportText", "Teleport")]
    private static partial string TeleportText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportAreaEnterPointBA_Name", "Teleport to AreaEnterPoint")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportAreaEnterPointBA_Description", "Teleports you to a specified BlueprintAreaEnterPoint.")]
    public override partial string Description { get; }
}
