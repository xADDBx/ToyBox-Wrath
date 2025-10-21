using Kingmaker.Designers;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Globalmap.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class TeleportGlobalMapPointBA : BlueprintActionFeature, IBlueprintAction<BlueprintGlobalMapPoint> {
    public bool CanExecute(BlueprintGlobalMapPoint blueprint, params object[] parameter) => IsInGame();
    private bool Execute(BlueprintGlobalMapPoint blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        GameHelper.EnterToArea(blueprint.GlobalMap.GlobalMapEnterPoint, AutoSaveMode.None);
        return true;
    }
    public bool? OnGui(BlueprintGlobalMapPoint blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(StyleActionString(TeleportText, isFeatureSearch), () => {
                result = Execute(blueprint, parameter);
            });
        } else if (isFeatureSearch) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
        }
        return result;
    }

    public bool GetContext(out BlueprintGlobalMapPoint? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!, true);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportGlobalMapPointBA_TeleportText", "Teleport")]
    private static partial string TeleportText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportGlobalMapPointBA_Name", "Teleport to GlobalMapPoint")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportGlobalMapPointBA_Description", "Teleports you to the specified BlueprintGlobalMapPoint.")]
    public override partial string Description { get; }
}
