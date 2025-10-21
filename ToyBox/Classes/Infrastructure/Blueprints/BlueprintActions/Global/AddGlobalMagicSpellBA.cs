using Kingmaker;
using Kingmaker.Crusade.GlobalMagic;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class AddGlobalMagicSpellBA : BlueprintActionFeature, IBlueprintAction<BlueprintGlobalMagicSpell> {
    public bool CanExecute(BlueprintGlobalMagicSpell blueprint, params object[] parameter) {
        return IsInGame()
            && !Game.Instance.Player.GlobalMapSpellsManager.SpellBook.Any(spell => spell.Blueprint == blueprint);
    }
    private bool Execute(BlueprintGlobalMagicSpell blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        Game.Instance.Player.GlobalMapSpellsManager.AddSpell(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintGlobalMagicSpell blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(StyleActionString(AddText, isFeatureSearch), () => {
                result = Execute(blueprint, parameter);
            });
        } else if (isFeatureSearch) {
            if (IsInGame()) {
                UI.Label(GlobalSpellIsAlreadyKnownText.Red().Bold());
            } else {
                UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
            }
        }
        return result;
    }
    public bool GetContext(out BlueprintGlobalMagicSpell? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!, true);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddGlobalMagicSpellBA_AddText", "Add")]
    private static partial string AddText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddGlobalMagicSpellBA_Name", "Add Global Spell")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddGlobalMagicSpellBA_Description", "Adds the specified BlueprintGlobalMagicSpell.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddGlobalMagicSpellBA_GlobalSpellIsAlreadyKnownText", "Global Spell is already known")]
    private static partial string GlobalSpellIsAlreadyKnownText { get; }
}
