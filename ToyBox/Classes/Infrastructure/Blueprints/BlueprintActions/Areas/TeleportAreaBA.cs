using Kingmaker.Blueprints.Area;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Persistence;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class TeleportAreaBA : BlueprintActionFeature, IBlueprintAction<BlueprintArea> {
    private bool CanExecute(BlueprintArea blueprint, params object[] parameter) {
        return IsInGame();
    }
    private bool Execute2(BlueprintArea blueprint, IEnumerable<BlueprintAreaEnterPoint> enterPoints, params object[] parameter) {
        var enterPoint = enterPoints.FirstOrDefault(bp => bp is BlueprintAreaEnterPoint ep && ep.Area == blueprint);
        LogExecution(blueprint, parameter, enterPoint);
        if (enterPoint != null) {
            GameHelper.EnterToArea(enterPoint, AutoSaveMode.None);
            return true;
        } else {
            return false;
        }
    }
    private bool Execute(BlueprintArea blueprint, params object[] parameter) {
        var areaEnterPoints = BPLoader.GetBlueprintsOfType<BlueprintAreaEnterPoint>(enterPoints => Main.ScheduleForMainThread(() => Execute2(blueprint, enterPoints, parameter)));
        if (areaEnterPoints != null) {
            Execute2(blueprint, areaEnterPoints, parameter);
            return true;
        }
        return false;
    }
    public bool? OnGui(BlueprintArea blueprint, bool isFeatureSearch, params object[] parameter) {
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

    public bool GetContext(out BlueprintArea? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!, true);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportAreaBA_TeleportText", "Teleport")]
    private static partial string TeleportText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportAreaBA_Name", "Teleport to Area")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportAreaBA_Description", "Teleports you to a random enter point of the specified BlueprintArea.")]
    public override partial string Description { get; }
}
