using Kingmaker;
using Kingmaker.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class ChangeFlagValueBA : BlueprintActionFeature, IBlueprintAction<BlueprintUnlockableFlag> {

    private bool CanExecute(BlueprintUnlockableFlag blueprint) {
        return IsInGame() && Game.Instance.Player.UnlockableFlags.IsUnlocked(blueprint);
    }
    private bool ExecuteIncrease(BlueprintUnlockableFlag blueprint, int count) {
        LogExecution(blueprint, count);
        Game.Instance.Player.UnlockableFlags.SetFlagValue(blueprint, Game.Instance.Player.UnlockableFlags.GetFlagValue(blueprint) + count);
        return true;
    }
    private bool ExecuteDecrease(BlueprintUnlockableFlag blueprint, int count) {
        LogExecution(blueprint, -count);
        Game.Instance.Player.UnlockableFlags.SetFlagValue(blueprint, Game.Instance.Player.UnlockableFlags.GetFlagValue(blueprint) - count);
        return true;
    }
    public bool? OnGui(BlueprintUnlockableFlag blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            int count = 1;
            if (parameter.Length > 0 && parameter[0] is int tmpCount) {
                count = tmpCount;
            }
            UI.Button("<", () => {
                result = ExecuteDecrease(blueprint, count);
            });
            UI.Label($" {Game.Instance.Player.UnlockableFlags.GetFlagValue(blueprint)} ".Orange().Bold());
            UI.Button(">", () => {
                result = ExecuteIncrease(blueprint, count);
            });
        }
        return result;
    }

    public bool GetContext(out BlueprintUnlockableFlag? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!);
        }
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeFlagValueBA_Name", "Modify Flag Value")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeFlagValueBA_Description", "Increases or decreases the value of the specified BlueprintUnlockableFlag.")]
    public override partial string Description { get; }
}
