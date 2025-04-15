using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public static partial class UI {
    public static void Label(string? title = null, params GUILayoutOption[] options) {
        options = options.Length == 0 ? [GUILayout.ExpandWidth(false)] : options;
        GUILayout.Label(title ?? "", options);
    }
}
