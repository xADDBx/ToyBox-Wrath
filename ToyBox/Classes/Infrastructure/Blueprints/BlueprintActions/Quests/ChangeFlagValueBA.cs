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
    public bool? OnGui(BlueprintUnlockableFlag blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            int count = 1;
            if (parameter.Length > 0 && parameter[0] is int tmpCount) {
                count = tmpCount;
            }
            var text1 = "<";
            var text2 = $" {Game.Instance.Player.UnlockableFlags.GetFlagValue(blueprint)} ".Bold().Orange();
            var text3 = ">";
            if (isFeatureSearch) {
                text1 = text1.Cyan().Bold().SizePercent(115);
                text2 = text2.SizePercent(115);
                text3 = text3.Cyan().Bold().SizePercent(115);
            }
            UI.Button(text1, () => {
                result = ExecuteDecrease(blueprint, count);
            });
            UI.Label(text2);
            UI.Button(text3, () => {
                result = ExecuteIncrease(blueprint, count);
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
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeFlagValueBA_Name", "Modify Flag Value")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeFlagValueBA_Description", "Increases or decreases the value of the specified BlueprintUnlockableFlag.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeFlagValueBA_FlagIsNotUnlockedText", "Flag is not unlocked")]
    private static partial string FlagIsNotUnlockedText { get; }
}
