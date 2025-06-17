using Kingmaker;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using ToyBox.Infrastructure;
using ToyBox.Infrastructure.Inspector;
using ToyBox.Infrastructure.Utilities;
using UnityEngine;

namespace ToyBox.Features.PartyTab;
public partial class PartyFeatureTab : FeatureTab {
    private TimedCache<Dictionary<UnitEntityData, float>> m_DistanceToCache = new(() => []);
    [LocalizedString("ToyBox_Features_PartyTab_PartyFeatureTab_PartyLevelText", "Party Level")]
    private static partial string PartyLevelText { get; }
    [LocalizedString("ToyBox_Features_PartyTab_PartyFeatureTab_InspectParty_forDebugging__Text", "Inspect Party (for debugging)")]
    private static partial string InspectPartyText { get; }
    [LocalizedString("ToyBox_Features_PartyTab_PartyFeatureTab_Name", "Party")]
    public override partial string Name { get; }
    public override void OnGui() {
        if (!IsInGame()) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red());
            return;
        }
        var units = CharacterPicker.OnFilterPickerGUI(6, GUILayout.ExpandWidth(true));
        using (VerticalScope()) {
            using (HorizontalScope()) {
                UI.Label((PartyLevelText + ": ").Cyan() + Game.Instance.Player.PartyLevel.ToString().Orange().Bold(), Width(150));
                InspectorUI.InspectToggle("Party", InspectPartyText, units, -150);
            }
            var mainChar = GameHelper.GetPlayerCharacter();
            foreach (var unit in units) {
                using (HorizontalScope()) {
                    UI.Label(ToyBoxUnitHelper.GetUnitName(unit).Orange().Bold());
                    Space(5);

                    UI.EditableLabel(unit.CharacterName, unit.UniqueId, newName => {
                        unit.Descriptor.CustomName = newName;
                        EventBus.RaiseEvent<IUnitNameHandler>(handler => handler.OnUnitNameChanged(unit));
                    });

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
