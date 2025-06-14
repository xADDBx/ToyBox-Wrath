using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public static partial class UI {
#warning TODO: Glyph Stuff Settings thingies
    private static string DefaultDisclosureOn = "▼";
    private static string DefaultDisclosureOff = "▶";
    private static GUIStyle CachedDisclosureToggleStyle {
        get {
            field ??= new GUIStyle(GUI.skin.label) { imagePosition = ImagePosition.ImageLeft, alignment = TextAnchor.MiddleLeft };
            return field;
        }
    }
    public static bool Toggle(string name, string description, ref bool setting, Action onEnable, Action onDisable, params GUILayoutOption[] options) {
        options = options.Length == 0 ? [AutoWidth()] : options;
        bool changed = false;
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(setting, name.Cyan(), options);
            if (newValue != setting) {
                changed = true;
                setting = newValue;
                if (newValue) {
                    onEnable();
                } else {
                    onDisable();
                }
            }
            Space(10);
            Label(description.Green());
        }
        return changed;
    }
    public static bool DisclosureToggle(ref bool state, string? name = null, params GUILayoutOption[] options) {
        options = options.Length == 0 ? [AutoWidth()] : options;
        string glyph = state ? DefaultDisclosureOn : DefaultDisclosureOff;
        var newValue = GUILayout.Toggle(state, glyph + (name ?? ""), CachedDisclosureToggleStyle, options);
        if (newValue != state) {
            state = newValue;
            return true;
        } else {
            return false;
        }
    }
}
