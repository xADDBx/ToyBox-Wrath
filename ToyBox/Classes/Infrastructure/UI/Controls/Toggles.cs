using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public static partial class UI {
#warning TODO: Glyph Stuff Settings thingies
    private static string DefaultDisclosureOn = "▼";
    private static string DefaultDisclosureOff = "▶";
    private static GUIStyle? m_CachedDisclosureToggleStyle;
    public static bool Toggle(string name, string description, ref bool setting, Action onEnable, Action onDisable, params GUILayoutOption[] options) {
        options = options.Length == 0 ? [AutoWidth()] : options;        bool changed = false;
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(setting, name.Cyan(), options);
            if (newValue != setting) {                changed = true;
                setting = newValue;
                if (newValue) {
                    onEnable();
                } else {
                    onDisable();
                }
            }
            Space(10);
            Label(description.Green());
        }        return changed;
    }
    public static bool DisclosureToggle(ref bool state, string? name = null, params GUILayoutOption[] options) {
        options = options.Length == 0 ? [AutoWidth()] : options;
        if (m_CachedDisclosureToggleStyle == null) {
            m_CachedDisclosureToggleStyle = new GUIStyle(GUI.skin.toggle) { imagePosition = ImagePosition.TextOnly };
            m_CachedDisclosureToggleStyle.onNormal.background = null;
            m_CachedDisclosureToggleStyle.normal.background = null;
            m_CachedDisclosureToggleStyle.onHover.background = null;
            m_CachedDisclosureToggleStyle.hover.background = null;
            m_CachedDisclosureToggleStyle.onFocused.background = null;
            m_CachedDisclosureToggleStyle.focused.background = null;
            m_CachedDisclosureToggleStyle.onActive.background = null;
            m_CachedDisclosureToggleStyle.active.background = null;
            m_CachedDisclosureToggleStyle.alignment = TextAnchor.MiddleLeft;
        }
        string glyph = state ? DefaultDisclosureOn : DefaultDisclosureOff;
        var newValue = GUILayout.Toggle(state, glyph + (name ?? ""), m_CachedDisclosureToggleStyle, options);
        if (newValue != state) {
            state = newValue;
            return true;
        } else {
            return false;
        }
    }
}
