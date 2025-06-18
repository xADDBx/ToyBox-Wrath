using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public static partial class UI {
    public static bool Slider(ref int value, float minValue, float maxValue, int? defaultValue = null, Action<(int oldValue, int newValue)>? onValueChanged = null, params GUILayoutOption[] options) {
        options = options.Length == 0 ? [AutoWidth(), Width(600)] : options;
        var oldValue = value;
        int result = (int)Math.Round(GUILayout.HorizontalSlider(oldValue, minValue, maxValue, options), 0);
        Label(value.ToString().Orange() + " ");
        if (defaultValue != null) {
            Space(4);
            Button(SharedStrings.ResetToDefault, () => {
                result = defaultValue.Value;
            });
        }
        if (result != value) {
            value = result;
            onValueChanged?.Invoke((oldValue, value));
            return true;
        }
        return false;
    }
    public static bool Slider(ref float value, float minValue, float maxValue, float? defaultValue = null, int digits = 2, Action<(float oldValue, float newValue)>? onValueChanged = null, params GUILayoutOption[] options) {
        options = options.Length == 0 ? [AutoWidth(), Width(600)] : options;
        var oldValue = value;
        float result = (float)Math.Round(GUILayout.HorizontalSlider(oldValue, minValue, maxValue, options), digits);
        Label(value.ToString().Orange() + " ");
        if (defaultValue != null) {
            Space(4);
            Button(SharedStrings.ResetToDefault, () => {
                result = defaultValue.Value;
            });
        }
        if (result != value) {
            value = result;
            onValueChanged?.Invoke((oldValue, value));
            return true;
        }
        return false;
    }
}
