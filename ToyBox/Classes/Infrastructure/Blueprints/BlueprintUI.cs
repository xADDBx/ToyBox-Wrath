using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using ToyBox.Infrastructure.Blueprints.BlueprintActions;

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
    public static void BlueprintRowGUI<Blueprint>(Blueprint blueprint, UnitEntityData ch, object? parent = null) where Blueprint : BlueprintScriptableObject, IUIDataProvider {
        string name;
        parent ??= ch;
        object? maybeItem = null;
        if (blueprint is BlueprintUnitFact fact) {
            maybeItem = ch.GetFact(fact);
        }
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
        var maybeSelection = blueprint as BlueprintFeatureSelection;
        var maybeParameterized = blueprint as BlueprintParametrizedFeature;
        bool hasUncollapsedChild = false;
        using (VerticalScope()) {
            using (HorizontalScope()) {
                if (maybeSelection != null || maybeParameterized != null) {
                    (object parent, BlueprintScriptableObject blueprint) key = (parent, blueprint);
                    if (!m_DisclosureStates.TryGetValue(key, out hasUncollapsedChild)) {
                        m_DisclosureStates[key] = hasUncollapsedChild = false;
                    }
                    if (UI.DisclosureToggle(ref hasUncollapsedChild, name, Width(CalculateTitleWidth()))) {
                        m_DisclosureStates[key] = hasUncollapsedChild;
                        if (!hasUncollapsedChild) {
                            m_SelectionBrowsers.Remove(key);
                            m_ParameterizedBrowser.Remove(key);
                        }
                    }
                } else {
                    UI.Label(name, Width(CalculateTitleWidth()));
                }

                foreach (var action in BlueprintActionFeature.GetActionsForBlueprintType<Blueprint>()) {
                    action.OnGui(blueprint, false, ch);
                }

                Space(10);

                var desc = BPHelper.GetDescription(blueprint);
                if (!desc.IsNullOrEmpty()) {
                    UI.Label(desc!.Green());
                }
            }

            if (hasUncollapsedChild) {
                if (maybeSelection != null) {
                    BlueprintRowGUI(maybeSelection, ch, parent);
                } else {
                    BlueprintRowGUI(maybeParameterized!, ch, parent);
                }
            }
        }
    }
    public static void BlueprintRowGUI(BlueprintFeatureSelection selection, UnitEntityData ch, object parent) {
        UI.Label("Selection");
        return;
        var data = ch.Progression.GetSelectionData(selection);
        if (!m_SelectionBrowsers.TryGetValue((parent, selection), out var browser)) {
            m_SelectionBrowsers[(parent, selection)] = browser = new(BPHelper.GetSortKey, BPHelper.GetSearchKey, data.SelectionsByLevel.SelectMany(levelPair => levelPair.Value), func => func(selection.AllFeatures), false);
        }
        Space(25);
        browser.OnGUI(feature => {
            int? featureLevel = GetLevelFeatureWasSelectedAt(data, feature);
            var name = BPHelper.GetTitle(feature);
            if (featureLevel != null) {
                name = name.Cyan().Bold();
            }
            var parameterized = feature as BlueprintParametrizedFeature;
            bool hasUncollapsedChild = false;
            using (VerticalScope()) {
                using (HorizontalScope()) {
                    if (parameterized != null) {
                        (object parent, BlueprintScriptableObject blueprint) key = (parent, feature);
                        if (!m_DisclosureStates.TryGetValue(key, out hasUncollapsedChild)) {
                            m_DisclosureStates[key] = hasUncollapsedChild = false;
                        }
                        if (UI.DisclosureToggle(ref hasUncollapsedChild, name, Width(CalculateTitleWidth()))) {
                            m_DisclosureStates[key] = hasUncollapsedChild;
                            if (!hasUncollapsedChild) {
                                m_ParameterizedBrowser.Remove(key);
                            }
                        }
                    } else {
                        UI.Label(name, Width(CalculateTitleWidth()));
                    }

                    // Put actions here
                }

                if (hasUncollapsedChild) {
                    BlueprintRowGUI(parameterized!, ch, parent);
                }
            }
        });
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
        UI.Label("Parametrized");
        return;
    }
}
