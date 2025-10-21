using Kingmaker;
using Kingmaker.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class UnlockFlagBA : BlueprintActionFeature, IBlueprintAction<BlueprintUnlockableFlag> {

    public bool CanExecute(BlueprintUnlockableFlag blueprint, params object[] parameter) {
        return IsInGame()
            && !Game.Instance.Player.UnlockableFlags.IsUnlocked(blueprint);
    }
    private bool Execute(BlueprintUnlockableFlag blueprint) {
        LogExecution(blueprint);
        Game.Instance.Player.UnlockableFlags.Unlock(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintUnlockableFlag blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            UI.Button(StyleActionString(UnlockText, isFeatureSearch), () => {
                result = Execute(blueprint);
            });
        } else if (isFeatureSearch) {
            if (IsInGame()) {
                UI.Label(FlagIsNotLockedText.Red().Bold());
            } else {
                UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
            }
        }
        return result;
    }

    public bool GetContext(out BlueprintUnlockableFlag? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!, true);
        }
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_UnlockFlagBA_Name", "Unlock Flag")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_UnlockFlagBA_Description", "Unlocks the specified BlueprintUnlockableFlag.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_UnlockFlagBA_UnlockText", "Unlock")]
    private static partial string UnlockText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_UnlockFlagBA_FlagIsNotLockedText", "Flag is not locked")]
    private static partial string FlagIsNotLockedText { get; }
}
