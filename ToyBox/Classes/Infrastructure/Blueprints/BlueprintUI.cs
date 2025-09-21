using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;

namespace ToyBox.Infrastructure.Blueprints;
public static class BlueprintUI {
    private static readonly Dictionary<(object parent, BlueprintScriptableObject key), bool> m_DisclosureStates = [];
    private static readonly Dictionary<(object parent, BlueprintScriptableObject key), Browser<BlueprintFeature>> m_SelectionBrowsers = [];
    private static readonly Dictionary<(object parent, BlueprintScriptableObject key), Browser<BlueprintParametrizedFeature>> m_ParameterizedBrowser = [];
    static BlueprintUI() {
        Main.OnHideGUIAction += ClearHideCaches;
    }
    private static void ClearHideCaches() {
        m_DisclosureStates.Clear();
        m_SelectionBrowsers.Clear();
        m_ParameterizedBrowser.Clear();
    }
    public static void BlueprintRowGUI<Item, Blueprint>(Item? maybeItem, Blueprint blueprint, UnitEntityData ch, object? parent = null) where Blueprint : BlueprintScriptableObject, IUIDataProvider {
        string name;
        parent ??= ch;
        if (maybeItem is AbilityData maybeSpell2 && maybeSpell2.Blueprint.IsSpell && maybeSpell2.MagicHackData != null) {
            name = maybeSpell2.MagicHackData.Name;
            if (string.IsNullOrEmpty(name)) {
                name = maybeSpell2.MagicHackData.GetDefaultName();
            }
        } else {
            name = BPHelper.GetTitle(blueprint);
        }
        if (maybeItem != null) {
            name = name.Cyan().Bold();
        }
        var titleWidth = CalculateTitleWidth();
        BlueprintFeatureSelection? maybeSelection = blueprint as BlueprintFeatureSelection;
        BlueprintParametrizedFeature? maybeParameterized = blueprint as BlueprintParametrizedFeature;
        if (maybeSelection != null || maybeParameterized != null) {
            (object parent, BlueprintScriptableObject blueprint) key = (parent, blueprint);
            if (!m_DisclosureStates.TryGetValue(key, out var tmpBool)) {
                m_DisclosureStates[key] = tmpBool = false;
            }
            if (UI.DisclosureToggle(ref tmpBool, name, Width(titleWidth))) {
                m_DisclosureStates[key] = tmpBool;
                if (!tmpBool) {
                    m_SelectionBrowsers.Remove(key);
                    m_ParameterizedBrowser.Remove(key);
                }
            }
            if (tmpBool) {
                if (maybeSelection != null) {
                    BlueprintRowGUI(maybeSelection, ch, parent);
                } else {
                    BlueprintRowGUI(maybeParameterized!, ch, parent);
                }
            }
        } else {
            UI.Label(name, Width(titleWidth));
        }
    }
    public static void BlueprintRowGUI(BlueprintFeatureSelection selection, UnitEntityData ch, object parent) {
        var data = ch.Progression.GetSelectionData(selection);
        if (!m_SelectionBrowsers.TryGetValue((parent, selection), out var browser)) {
            m_SelectionBrowsers[(parent, selection)] = browser = new(BPHelper.GetSortKey, BPHelper.GetSearchKey, data.SelectionsByLevel.SelectMany(levelPair => levelPair.Value), func => func(selection.AllFeatures), false);
        }
        using (HorizontalScope()) {
            Space(25);
            browser.OnGUI(feature => {
                int? featureLevel = GetLevelFeatureWasSelectedAt(data, feature);
                var title = BPHelper.GetTitle(feature);
                if (featureLevel != null) {
                    title = title.Cyan().Bold();
                }
                UI.Label();
            });
        }
    }
    private static float CalculateTitleWidth() => Math.Min(300, EffectiveWindowWidth() * 0.2f);
    private static int? GetLevelFeatureWasSelectedAt(FeatureSelectionData data, BlueprintFeature feature) {
        foreach (var pair in data.m_SelectionsByLevel) {
            if (pair.Value.Contains(feature)) {
                return pair.Key;
            }
        }
        return null;
    }
    public static void BlueprintRowGUI(BlueprintParametrizedFeature parameterized, UnitEntityData ch, object parent) {

    }
}
