using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace ToyBox.Infrastructure.Blueprints;
public static class BlueprintUI {
    private static Dictionary<(object parent, BlueprintScriptableObject key), bool> m_DisclosureStates = new();
    private static Dictionary<(object parent, BlueprintScriptableObject key), Browser<FeatureSelectionData>> m_SelectionBrowsers = new();
    private static Dictionary<(object parent, BlueprintScriptableObject key), Browser<BlueprintParametrizedFeature>> m_ParameterizedBrowser = new();
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
        var titleWidth = Math.Min(300, EffectiveWindowWidth() * 0.2f);
        BlueprintFeatureSelection? maybeSelection = blueprint as BlueprintFeatureSelection;
        BlueprintParametrizedFeature? maybeParameterized = blueprint as BlueprintParametrizedFeature;
        if (maybeSelection != null || maybeParameterized != null) {
            if (!m_DisclosureStates.TryGetValue((parent, blueprint), out var tmpBool)) {
                m_DisclosureStates[(parent, blueprint)] = tmpBool = false;
            }
            if (UI.UI.DisclosureToggle(ref tmpBool, name, Width(titleWidth))) {
                m_DisclosureStates[(parent, blueprint)] = tmpBool;
            }
            if (tmpBool) {
                if (maybeSelection != null) {
                    BlueprintRowGUI(maybeSelection, ch, parent);
                } else {
                    BlueprintRowGUI(maybeParameterized!, ch, parent);
                }
            }
        } else {
            UI.UI.Label(name, Width(titleWidth));
        }
    }
    public static void BlueprintRowGUI(BlueprintFeatureSelection selection, UnitEntityData ch, object parent) {
        if (!m_SelectionBrowsers.TryGetValue((parent, selection), out var browser)) {
            m_SelectionBrowsers[(parent, selection)] = browser = new();
        }
        browser.OnGUI(() => {

        })
    }
    public static void BlueprintRowGUI(BlueprintParametrizedFeature parameterized, UnitEntityData ch, object parent) {

    }
}
