using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public static class LayoutHelper {
    public static bool ImguiCanChangeStateAtBeginning() => Event.current.type == EventType.Layout;
    public static bool ImguiCanChangeStateAtEnd() => Event.current.type == EventType.Repaint;
    public static GUILayout.HorizontalScope HorizontalScope(params GUILayoutOption[] options) => new(options);
    public static GUILayout.HorizontalScope HorizontalScope(float width) => new(GUILayout.Width(width));
    public static GUILayout.HorizontalScope HorizontalScope(GUIStyle style, params GUILayoutOption[] options) => new(style, options);
    public static GUILayout.HorizontalScope HorizontalScope(GUIStyle style, float width) => new(style, GUILayout.Width(width));
    public static GUILayout.VerticalScope VerticalScope(params GUILayoutOption[] options) => new(options);
    public static GUILayout.VerticalScope VerticalScope(float width) => new(GUILayout.Width(width));
    public static GUILayout.VerticalScope VerticalScope(GUIStyle style, params GUILayoutOption[] options) => new(style, options);
    public static GUILayout.VerticalScope VerticalScope(GUIStyle style, float width) => new(style, GUILayout.Width(width));
    public static GUILayoutOption Width(float width) => GUILayout.Width(width);
    public static GUILayoutOption Height(float height) => GUILayout.Height(height);
    public static void Space(float pixels) => GUILayout.Space(pixels);
}
