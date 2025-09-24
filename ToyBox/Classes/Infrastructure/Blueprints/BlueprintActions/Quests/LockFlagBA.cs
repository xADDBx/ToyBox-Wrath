using Kingmaker;
using Kingmaker.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class LockFlagBA : BlueprintActionFeature, IBlueprintAction<BlueprintUnlockableFlag> {

    private bool CanExecute(BlueprintUnlockableFlag blueprint) {
        return IsInGame() && Game.Instance.Player.UnlockableFlags.IsUnlocked(blueprint);
    }
    private bool Execute(BlueprintUnlockableFlag blueprint) {
        LogExecution(blueprint);
        Game.Instance.Player.UnlockableFlags.Lock(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintUnlockableFlag blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            var text = LockText;
            if (isFeatureSearch) {
                text = text.Cyan().Bold().SizePercent(115);
            }
            UI.Button(text, () => {
                result = Execute(blueprint);
            });
        } else if (isFeatureSearch) {
            if (IsInGame()) {
                UI.Label(FlagIsNotUnlockedText.Red().Bold());
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
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_LockFlagBA_Name", "Lock Flag")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_LockFlagBA_Description", "Locks the specified BlueprintUnlockableFlag.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_LockFlagBA_LockText", "Lock")]
    private static partial string LockText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_LockFlagBA_FlagIsNotUnlockedText", "Flag is not unlocked")]
    private static partial string FlagIsNotUnlockedText { get; }
}
