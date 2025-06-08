using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using ToyBox.Infrastructure;
using ToyBox.Infrastructure.Utilities;
using UnityEngine;

namespace ToyBox.Features.PartyTab;
public partial class PartyFeatureTab : FeatureTab {
    private TimedCache<Dictionary<UnitEntityData, float>> m_DistanceToCache = new(() => []);
    [LocalizedString("ToyBox_Features_PartyTab_PartyFeatureTab_Name", "Party")]
    public override partial string Name { get; }
    public override void OnGui() {
        if (!IsInGame()) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red());
            return;
        }
        var units = CharacterPicker.OnFilterPickerGUI(6, GUILayout.ExpandWidth(true));
        using (VerticalScope()) {
#warning Inspect Party + Current Party Level
            var mainChar = GameHelper.GetPlayerCharacter();
            foreach (var unit in units) {
                using (HorizontalScope()) {
                    UI.Label(ToyBoxUnitHelper.GetUnitName(unit).Orange().Bold());
                    Space(5);
#warning rename feature; unique id based!

                    // Space(5);
                    Dictionary<UnitEntityData, float> distanceCache = m_DistanceToCache;
                    if (!distanceCache.TryGetValue(unit, out float dist)) {
                        dist = mainChar.DistanceTo(unit);
                        distanceCache[unit] = dist;
                    }
                    UI.Label(dist < 1 ? "" : dist.ToString("0") + "m", Width(75));
                    Space(5);
                }
            }
        }
    }
}
