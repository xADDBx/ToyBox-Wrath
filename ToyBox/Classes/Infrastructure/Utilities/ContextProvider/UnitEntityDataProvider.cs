using Kingmaker.EntitySystem.Entities;

namespace ToyBox.Infrastructure.Utilities;
public static partial class ContextProvider {
    public static bool UnitEntityData(out UnitEntityData? unit) {
        unit = null;
        if (!IsInGame()) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red());
            return false;
        }
        CharacterPicker.OnFilterPickerGUI();
        CharacterPicker.OnCharacterPickerGUI();
        if (CharacterPicker.CurrentUnit != null) {
            unit = CharacterPicker.CurrentUnit;
        } else {
            UI.Label(SharedStrings.PleaseSelectAUnitFirstText.Orange());
        }
        return unit != null;
    }
}
