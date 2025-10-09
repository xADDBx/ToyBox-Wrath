using Kingmaker.Designers;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Globalmap.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class TeleportGlobalMapBA : BlueprintActionFeature, IBlueprintAction<BlueprintGlobalMap> {
    private bool CanExecute(BlueprintGlobalMap blueprint, params object[] parameter) {
        return IsInGame();
    }
    private bool Execute(BlueprintGlobalMap blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        GameHelper.EnterToArea(blueprint.GlobalMapEnterPoint, AutoSaveMode.None);
        return true;
    }
    public bool? OnGui(BlueprintGlobalMap blueprint, bool isFeatureSearch, params object[] parameter) {
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

    public bool GetContext(out BlueprintGlobalMap? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!, true);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportGlobalMapBA_TeleportText", "Teleport")]
    private static partial string TeleportText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportGlobalMapBA_Name", "Teleport to GlobalMap")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportGlobalMapBA_Description", "Teleports you to the specified BlueprintGlobalMap.")]
    public override partial string Description { get; }
}
