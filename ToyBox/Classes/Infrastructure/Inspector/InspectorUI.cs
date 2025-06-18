using UnityEngine;

namespace ToyBox.Infrastructure.Inspector;
public static class InspectorUI {
    private static GUIStyle m_ButtonStyle {
        get {
            field ??= new(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, stretchHeight = false };
            return field;
        }
    }
    private static readonly Dictionary<object, InspectorNode> m_CurrentlyInspecting = [];
    private static readonly HashSet<object> m_ExpandedKeys = [];
    static InspectorUI() {
        Main.OnHideGUIAction += ClearCache;
    }
    public static void ClearCache() {
        m_CurrentlyInspecting.Clear();
        m_ExpandedKeys.Clear();
    }
    public static void InspectToggle(object key, string? title = null, object? toInspect = null, int indent = 0) {
        using (VerticalScope()) {
            title ??= key.ToString();
            toInspect ??= key;
            var expanded = m_ExpandedKeys.Contains(key);
            if (UI.UI.DisclosureToggle(ref expanded, title)) {
                if (expanded) {
                    m_ExpandedKeys.Clear();
                    m_ExpandedKeys.Add(key);
                } else {
                    m_ExpandedKeys.Remove(key);
                }
            }
            if (expanded) {
                using (HorizontalScope()) {
                    Space(indent);
                    Inspect(toInspect);
                }
            }
        }
    }
    public static void Inspect(object? obj) {
        using (VerticalScope()) {
            if (obj == null) {
                UI.UI.Label(SharedStrings.CurrentlyInspectingText + ": " + "<null>".Cyan());
            } else {
                var valueText = "";
                try {
                    valueText = obj.ToString();
                } catch (Exception ex) {
                    Warn($"Encountered exception in Inspect -> obj.ToString():\n{ex}");
                }
                UI.UI.Label(SharedStrings.CurrentlyInspectingText + ": " + valueText.Cyan());
                if (!m_CurrentlyInspecting.TryGetValue(obj, out InspectorNode root)) {
                    root = InspectorTraverser.BuildRoot(obj);
                    InspectorTraverser.BuildChildren(root);
                    m_CurrentlyInspecting[obj] = root;
                }
                foreach (var child in root.Children!) {
                    DrawNode(child, 1);
                }
            }
        }
    }

    public static void DrawNode(InspectorNode node, int indent) {
        using (HorizontalScope()) {
            GUILayout.Space(indent * Settings.InspectorIndentWidth);
            if (!Settings.ToggleInspectorShowNullAndEmptyMembers && node.IsNull) {
                return;
            }
            if (node.Children == null) {
                InspectorTraverser.BuildChildren(node);
            }
            if (!Settings.ToggleInspectorShowNullAndEmptyMembers && node.IsEnumerable && node.Children!.Count == 0) {
                return;
            }

            var discWidth = UI.UI.DisclosureGlyphWidth.Value;
            var leftOverWidth = EffectiveWindowWidth() - (indent * Settings.InspectorIndentWidth) - 40 - discWidth;
            var calculatedWidth = Settings.InspectorNameFractionOfWidth * leftOverWidth;
            if (Settings.ToggleInspectorSlimMode) {
                calculatedWidth = Math.Min(calculatedWidth * leftOverWidth, node.OwnTextLength!.Value);
            }

            if (node.Children!.Count > 0) {
                UI.UI.DisclosureToggle(ref node.IsExpanded, node.NameText, Width(calculatedWidth + discWidth));
            } else {
                Space(discWidth);
                GUILayout.Label(node.NameText, Width(calculatedWidth));
            }

            if (Settings.ToggleInspectorSlimMode) {
                Space(10);
            }

            // TextArea does not parse color tags; so it needs this workaround to colour text
            var currentColor = GUI.contentColor;
            GUI.contentColor = node.ColorOverride ?? currentColor;
            GUILayout.TextArea(node.ValueText);
            GUI.contentColor = currentColor;
            if (node.AfterText != "") {
                GUILayout.Label(node.AfterText, m_ButtonStyle, AutoWidth());
            } else {
                UI.UI.Label("");
            }
        }
        if (node.IsExpanded) {
            foreach (var child in node.Children!) {
                DrawNode(child, indent + 1);
            }
        }
    }
}
