using Kingmaker.EntitySystem.Entities;

namespace ToyBox.Infrastructure.Utilities;
public static partial class ContextProvider {
    private static bool m_UnitProviderShown = false;
    public static bool UnitEntityData(out UnitEntityData? unit) {
        unit = CharacterPicker.CurrentUnit;
        if (!IsInGame()) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red());
            return false;
        }
        string str;
        if (unit != null) {
            str = ": " + $"{unit}".Green().Bold();
        } else {
            str = ": " + NoneText.Red();
        }
        using (VerticalScope()) {
            UI.DisclosureToggle(ref m_UnitProviderShown, SharedStrings.CurrentlySelectedUnitText + str);
            if (m_UnitProviderShown) {
                CharacterPicker.OnFilterPickerGUI();
                bool didChange = !CharacterPicker.OnCharacterPickerGUI();
                unit = CharacterPicker.CurrentUnit;
                m_UnitProviderShown = !didChange || (unit == null);
            }
        }
        return unit != null;
    }
}
