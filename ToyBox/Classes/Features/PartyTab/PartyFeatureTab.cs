using Kingmaker;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
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
    private static PartyTabSectionType m_UncollapsedSection = PartyTabSectionType.None;
    private static UnitEntityData? m_UncollapsedUnit = null;
    private static readonly PartyTabSectionType[] m_Sections = [PartyTabSectionType.Classes, PartyTabSectionType.Stats, PartyTabSectionType.Features,
        PartyTabSectionType.Buffs, PartyTabSectionType.Abilities, PartyTabSectionType.Spells, PartyTabSectionType.Inspect];
    public PartyFeatureTab() {
        AddFeature(new FeatureBrowserUnitFeature());
    }
    static PartyFeatureTab() {
        Main.OnHideGUIAction += Refresh;
    }
    public static void Refresh() {
        m_UncollapsedSection = PartyTabSectionType.None;
        m_UncollapsedUnit = null;
        m_NameSectionWidth.ForceRefresh();
    }
    private static readonly TimedCache<float> m_NameSectionWidth = new(() => {
        return CalculateLargestLabelSize(CharacterPicker.CurrentUnits.Select(u => u.CharacterName.Bold()));
    }, 60*60*24);
    public override void OnGui() {
        if (!IsInGame()) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red());
            return;
        }
        if (CharacterPicker.OnFilterPickerGUI(6, GUILayout.ExpandWidth(true))) {
            m_NameSectionWidth.ForceRefresh();
        }
        var units = CharacterPicker.CurrentUnits;
        using (VerticalScope()) {
            using (HorizontalScope()) {
                UI.Label((PartyLevelText + ": ").Cyan() + Game.Instance.Player.PartyLevel.ToString().Orange().Bold(), Width(150));
                InspectorUI.InspectToggle("Party", InspectPartyText, units, -150, true);
            }
            var mainChar = GameHelper.GetPlayerCharacter();
            foreach (var unit in units) {
                using (HorizontalScope()) {
                    using (HorizontalScope(Width(Math.Min(EffectiveWindowWidth() * 0.2f, m_NameSectionWidth + 110)))) {
                        UI.Label(ToyBoxUnitHelper.GetUnitName(unit).Orange().Bold(), Width(m_NameSectionWidth));
                        Space(2);

                        UI.EditableLabel(unit.CharacterName, unit.UniqueId, newName => {
                            unit.Descriptor.CustomName = newName;
                            EventBus.RaiseEvent<IUnitNameHandler>(handler => handler.OnUnitNameChanged(unit));
                            Main.ScheduleForMainThread(m_NameSectionWidth.ForceRefresh);
                        });

                        Dictionary<UnitEntityData, float> distanceCache = m_DistanceToCache;
                        if (!distanceCache.TryGetValue(unit, out float dist)) {
                            dist = mainChar.DistanceTo(unit);
                            distanceCache[unit] = dist;
                        }
                        Space(13);
                        UI.Label(dist < 1 ? "" : dist.ToString("0") + "m", Width(70));
                        GUILayout.FlexibleSpace();
                    }
                    foreach (var sec in m_Sections) {
                        bool isUncollapsed = sec == m_UncollapsedSection && unit == m_UncollapsedUnit;
                        if (UI.DisclosureToggle(ref isUncollapsed, " " + sec.GetLocalized())) {
                            if (isUncollapsed) {
                                m_UncollapsedSection = sec;
                                m_UncollapsedUnit = unit;
                            } else {
                                m_UncollapsedSection = PartyTabSectionType.None;
                                m_UncollapsedUnit = null;
                            }
                        }
                        GUILayout.FlexibleSpace();
                    }
                }
                if (m_UncollapsedUnit == unit && m_UncollapsedSection != PartyTabSectionType.None) {
                    using (HorizontalScope()) {
                        Space(10);
                        UI.Label("Uncollapsed Hurray");
                    }
                }
            }
        }
    }
}
