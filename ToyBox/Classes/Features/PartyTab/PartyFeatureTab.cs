using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using ToyBox.Infrastructure;
using UnityEngine;

namespace ToyBox.Features.PartyTab;
public partial class PartyFeatureTab : FeatureTab, IDisposable {
    private Dictionary<UnitEntityData, float> m_DistanceToCache = [];
    [LocalizedString("ToyBox_Features_PartyTab_PartyFeatureTab_Name", "Party")]
    public override partial string Name { get; }
    public PartyFeatureTab() {
        Main.OnHideGUIAction += ClearCache;
    }
    private void ClearCache() {
        m_DistanceToCache.Clear();
    }
    public override void OnGui() {
        if (!IsInGame()) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red());
            return;
        }
        CharacterPicker.OnFilterPickerGUI(null, GUILayout.ExpandWidth(true));
        CharacterPicker.OnCharacterPickerGUI(null, GUILayout.ExpandWidth(true));
        using (VerticalScope()) {
#warning Inspect Party + Current Party Level
            var mainChar = GameHelper.GetPlayerCharacter();
            foreach (var unit in CharacterPicker.CurrentList.Units) {
                using (HorizontalScope()) {
                    UI.Label(ToyBoxUnitHelper.GetUnitName(unit).Orange().Bold());
                    Space(5);
 #warning rename feature; unique id based!
                    
                    // Space(5);
                    if (!m_DistanceToCache.TryGetValue(unit, out var dist)) {
                        dist = mainChar.DistanceTo(unit);
                        m_DistanceToCache[unit] = dist;
                    }
                    UI.Label(dist < 1 ? "" : dist.ToString("0") + "m", Width(75));
                    Space(5);
                }
            }
        }
    }

    public void Dispose() {
        Main.OnHideGUIAction -= ClearCache;
    }
}
