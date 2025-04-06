using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public static partial class UI {
    public static void Toggle(string name, string description, ref bool setting, Action onEnable, Action onDisable) {
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(setting, name.Cyan(), GUILayout.ExpandWidth(false));
            if (newValue != setting) {
                setting = newValue;
                if (newValue) {
                    onEnable();
                } else {
                    onDisable();
                }
            }
            GUILayout.Space(10);
            GUILayout.Label(description.Green(), GUILayout.ExpandWidth(false));
        }
    }
}
