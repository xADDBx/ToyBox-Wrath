using Kingmaker;
using Kingmaker.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class UnlockFlagBA : BlueprintActionFeature, IBlueprintAction<BlueprintUnlockableFlag> {

    private bool CanExecute(BlueprintUnlockableFlag blueprint) {
        return IsInGame() && !Game.Instance.Player.UnlockableFlags.IsUnlocked(blueprint);
    }
    private bool Execute(BlueprintUnlockableFlag blueprint) {
        LogExecution(blueprint);
        Game.Instance.Player.UnlockableFlags.Unlock(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintUnlockableFlag blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            UI.Button(UnlockText, () => {
                result = Execute(blueprint);
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
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_UnlockFlagBA_Name", "Unlock Flag")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_UnlockFlagBA_Description", "Unlocks the specified BlueprintUnlockableFlag.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_UnlockFlagBA_UnlockText", "Unlock")]
    private static partial string UnlockText { get; }
}
