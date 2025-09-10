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
    public static bool LogSlider(ref int value, float minValue, float maxValue, int? defaultValue = null, Action<(int oldValue, int newValue)>? onValueChanged = null, params GUILayoutOption[] options) {
        options = options.Length == 0 ? [AutoWidth(), Width(600)] : options;
        var oldValue = value;
        const int offset = 1;

        float logValue = 100f * (float)Math.Log10(value + offset);
        float logMin = 100f * (float)Math.Log10(minValue + offset);
        float logMax = 100f * (float)Math.Log10(maxValue + offset);

        float logResult = GUILayout.HorizontalSlider(logValue, logMin, logMax, options);
        int result = (int)Math.Round(Math.Pow(10, logResult / 100f) - offset, 0);
        Label(value.ToString().Orange() + " ");
        if (defaultValue != null) {
            Space(4);
            Button(SharedStrings.ResetToDefault, () => {
                result = defaultValue.Value;
            });
        }
        if (Math.Abs((result - value)) > float.Epsilon) {
            value = result;
            onValueChanged?.Invoke((oldValue, value));
            return true;
        }
        return false;
    }
    public static bool LogSlider(ref float value, float minValue, float maxValue, float? defaultValue = null, int digits = 2, Action<(float oldValue, float newValue)>? onValueChanged = null, params GUILayoutOption[] options) {
        options = options.Length == 0 ? [AutoWidth(), Width(600)] : options;
        var oldValue = value;
        const int offset = 1;

        float logValue = 100f * (float)Math.Log10(value + offset);
        float logMin = 100f * (float)Math.Log10(minValue + offset);
        float logMax = 100f * (float)Math.Log10(maxValue + offset);

        float logResult = GUILayout.HorizontalSlider(logValue, logMin, logMax, options);
        float result = (float)Math.Round(Math.Pow(10, logResult / 100f) - offset, digits);
        Label(value.ToString().Orange() + " ");
        if (defaultValue != null) {
            Space(4);
            Button(SharedStrings.ResetToDefault, () => {
                result = defaultValue.Value;
            });
        }
        if (Math.Abs((result - value)) > float.Epsilon) {
            value = result;
            onValueChanged?.Invoke((oldValue, value));
            return true;
        }
        return false;
    }
}
