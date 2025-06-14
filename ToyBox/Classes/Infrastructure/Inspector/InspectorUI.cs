using ToyBox.Infrastructure.Utilities;
using UnityEngine;

namespace ToyBox.Infrastructure.Inspector;
public static partial class InspectorUI {
    [LocalizedString("ToyBox_Infrastructure_Inspector_InspectorUI_CurrentlyInspectingText", "Currently Inspecting")]
    private static partial string CurrentlyInspectingText { get; }
    private static GUIStyle ButtonStyle {
        get {
            field ??= new(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, stretchHeight = false };
            return field;
        }
    }
    private const float m_IndentWidth = 20f;
    private const float m_NameFractionOfWidth = 0.3f;
    private static readonly Dictionary<object, InspectorNode> m_CurrentlyInspecting = [];
    static InspectorUI() {
        Main.OnHideGUIAction += ClearCache;
    }
    public static void ClearCache() {
        m_CurrentlyInspecting.Clear();
    }
    public static void Inspect(object? obj) {
        using (VerticalScope()) {
            if (obj == null) {
                UI.UI.Label(CurrentlyInspectingText + ": " + "<null>".Cyan());
            } else {
                var valueText = "";
                try {
                    valueText = obj.ToString();
                } catch (Exception ex) {
                    Warn($"Encountered exception in Inspect -> obj.ToString():\n{ex}");
                }
                UI.UI.Label(CurrentlyInspectingText + ": " + valueText.Cyan());
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
            GUILayout.Space(indent * m_IndentWidth);
            if (!Settings.ToggleInspectorShowNullAndEmptyMembers && node.Value is null) {
                return;
            }
            if (node.Children == null) {
                InspectorTraverser.BuildChildren(node);
            }
            if (!Settings.ToggleInspectorShowNullAndEmptyMembers && node.IsEnumerable && node.Children!.Count == 0) {
                return;
            }

            string labelText = $"[{node.ContainerPrefix}] ".Grey();
            if (node.IsStatic) {
                labelText += "[s] ".Magenta();
            }
            labelText += node.Name;

            if (node.IsGameObject || node.IsEnumerable) {
                labelText += " " + $"[{node.ElementCount}]".Yellow();
            }
            string typeName = ToyBoxReflectionHelper.GetNameWithGenericsResolved(node.FieldType);
            if (ToyBoxReflectionHelper.PrimitiveTypes.Contains(node.ConcreteType)) {
                typeName = typeName.Grey();
            } else if (node.IsGameObject) {
                typeName = typeName.Magenta();
            } else if (node.IsEnumerable) {
                typeName = typeName.Cyan();
            } else {
                typeName = typeName.Orange();
            }
            labelText += " : " + typeName;

            var leftOverWidth = EffectiveWindowWidth() - (indent * m_IndentWidth) - 40;
            if (node.Children!.Count > 0) {
                UI.UI.DisclosureToggle(ref node.IsExpanded, labelText, Width(m_NameFractionOfWidth * leftOverWidth));
            } else {
                GUILayout.Label(labelText, Width(m_NameFractionOfWidth * leftOverWidth));
            }
            var valueText = "";
            var currentColor = GUI.contentColor;
            Color color = currentColor;
            if (node.Exception != null) {
                valueText = "<exception>";
                color = Color.red;
            } else {
                if (node.Value is null) {
                    valueText = "<null>";
                    color = Color.gray;
                } else {
                    try {
                        valueText = node.Value.ToString();
                    } catch (Exception ex) {
                        node.Exception = ex;
                        valueText = "<exception>";
                        color = Color.red;
                    }
                }
            }
            // TextArea does not parse color tags; so it needs this workaround to colour text
            GUI.contentColor = color;
            GUILayout.TextArea(valueText);
            GUI.contentColor = currentColor;

            if (node.ConcreteType != node.FieldType) {
                var text = ToyBoxReflectionHelper.GetNameWithGenericsResolved(node.ConcreteType).Yellow();
                GUILayout.Label(text, ButtonStyle, AutoWidth());
            } else {
                UI.UI.Label("");
            }
        }
        if (node.IsExpanded) {
            foreach (var child in node.Children) {
                DrawNode(child, indent + 1);
            }
        }
    }
}
