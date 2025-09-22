using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.View;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class TeleportAreaBA : IBlueprintAction<BlueprintArea> {
    static TeleportAreaBA() {
        BlueprintActions.RegisterAction((IBlueprintAction<SimpleBlueprint>)new TeleportAreaBA());
    }
    private bool CanExecute(BlueprintArea blueprint, params object[] parameter) {
        return IsInGame();
    }
    private bool Execute2(BlueprintArea blueprint, IEnumerable<BlueprintAreaEnterPoint> enterPoints, params object[] parameter) {
        var enterPoint = enterPoints.FirstOrDefault(bp => bp is BlueprintAreaEnterPoint ep && ep.Area == blueprint);
        ((IBlueprintAction<SimpleBlueprint>)this).LogBPAction(blueprint, parameter, enterPoint);
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
    public bool? OnGui(BlueprintArea blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(TeleportText, () => {
                result = Execute(blueprint, parameter);
            });
        }
        return result;
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_TeleportAreaBA_TeleportText", "Teleport")]
    private static partial string TeleportText { get; }
}
