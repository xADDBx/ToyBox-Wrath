using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public static class LayoutHelper {
    public static GUILayout.HorizontalScope HorizontalScope(params GUILayoutOption[] options) => new(options);
    public static GUILayout.HorizontalScope HorizontalScope(float width) => new(GUILayout.Width(width));
    public static GUILayout.HorizontalScope HorizontalScope(GUIStyle style, params GUILayoutOption[] options) => new(style, options);
    public static GUILayout.HorizontalScope HorizontalScope(GUIStyle style, float width) => new(style, GUILayout.Width(width));
    public static GUILayout.VerticalScope VerticalScope(params GUILayoutOption[] options) => new(options);
    public static GUILayout.VerticalScope VerticalScope(float width) => new(GUILayout.Width(width));
    public static GUILayout.VerticalScope VerticalScope(GUIStyle style, params GUILayoutOption[] options) => new(style, options);
    public static GUILayout.VerticalScope VerticalScope(GUIStyle style, float width) => new(style, GUILayout.Width(width));
}
