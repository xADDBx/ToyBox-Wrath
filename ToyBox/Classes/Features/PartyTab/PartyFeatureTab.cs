using ToyBox.Infrastructure;
using UnityEngine;

namespace ToyBox.Features.PartyTab;
public partial class PartyFeatureTab : FeatureTab {
    [LocalizedString("ToyBox_Features_PartyTab_PartyFeatureTab_Name", "Party")]
    public override partial string Name { get; }
    public PartyFeatureTab() {
    }
    public override void OnGui() {
        if (!IsInGame()) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red());
            return;
        }
        CharacterPicker.OnFilterPickerGUI(null, GUILayout.ExpandWidth(true));
        CharacterPicker.OnCharacterPickerGUI(null, GUILayout.ExpandWidth(true));
        using (VerticalScope()) {
            foreach (var unit in CharacterPicker.CurrentList.Units) {
                using (HorizontalScope()) {
                    UI.Label(ToyBoxUnitHelper.GetUnitName(unit));
                }
            }
        }
    }
}
