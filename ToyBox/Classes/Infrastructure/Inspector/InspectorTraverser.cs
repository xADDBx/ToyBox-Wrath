using System.Collections;
using System.Reflection;
using ToyBox.Infrastructure.Utilities;
using UnityEngine;

namespace ToyBox.Infrastructure.Inspector;
public static class InspectorTraverser {
    private static readonly BindingFlags m_All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;
    private static readonly BindingFlags m_AllInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;
    public static InspectorNode BuildRoot(object obj) {
        var type = obj?.GetType() ?? typeof(object);
        return new InspectorNode("root", "", type, obj, null, "");
    }
    public static void BuildChildren(InspectorNode node) {
        if (node.Children != null) {
            return;
        } else {
            BuildChildrenInternal(node);
            node.Children!.Sort();
        }
    }
    private static void BuildChildrenInternal(InspectorNode node) {
        node.Children = [];

        if (node.Value == null) {
            return;
        }

        if (ToyBoxReflectionHelper.PrimitiveTypes.Contains(node.ConcreteType) || node.ConcreteType.IsEnum) {
            return;
        }

        if (node.Value is IEnumerable enumerable) {
            Type? elementType = null;
            var collectionType = node.Value.GetType();
            if (collectionType.IsArray) {
                elementType = collectionType.GetElementType()!;
            } else {
                var genericEnumerable = collectionType
                    .GetInterfaces()
                    .FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (genericEnumerable != null) {
                    elementType = genericEnumerable.GetGenericArguments()[0];
                }
            }
            int index = 0;
            foreach (object? element in enumerable) {
                var childNode = new InspectorNode("<item_" + index + ">", node.Path, element?.GetType() ?? elementType ?? typeof(object), element, node, InspectorNode.EnumerableItemPrefix);
                node.Children.Add(childNode);
                index++;
            }
            if (!Settings.ToggleInspectorShowFieldsOnEnumerable) {
                return;
            }
        }

        if (node.Value is GameObject go) {
            int index = 0;
            foreach (var comp in go.GetComponents<Component>()) {
                var childNode = new InspectorNode("<component_" + index + ">", node.Path, comp?.GetType() ?? typeof(Component), comp, node, InspectorNode.GameObjectComponentPrefix);
                node.Children.Add(childNode);
                index++;
            }
            foreach (GameObject child in go.transform) {
                var childNode = new InspectorNode("<child_" + index + ">", node.Path, child?.GetType() ?? typeof(GameObject), child, node, InspectorNode.GameObjectChildPrefix);
                node.Children.Add(childNode);
                index++;
            }
            return;
        }

        foreach (var field in node.ConcreteType.GetFields(Settings.ToggleInspectorShowStaticMembers ? m_All : m_AllInstance)) {
            object? fieldValue;
            if (field.IsStatic) {
                fieldValue = field.GetValue(null);
            } else {
                fieldValue = field.GetValue(node.Value);
            }
            var childNode = new InspectorNode(field.Name, node.Path, field.FieldType, fieldValue, node, InspectorNode.FieldPrefix, field.IsStatic, field.IsPublic, field.IsPrivate);
            node.Children.Add(childNode);
        }

        foreach (var prop in node.ConcreteType.GetProperties(Settings.ToggleInspectorShowStaticMembers ? m_All : m_AllInstance)) {
            var getter = prop.GetMethod;
            if (getter == null || getter.GetParameters().Length > 0) {
                continue;
            }
            object? propValue = null;
            Exception? exception = null;
            try {
                if (getter.IsStatic) {
                    propValue = getter.Invoke(null, null);
                } else {
                    propValue = getter.Invoke(node.Value, null);
                }
            } catch (Exception ex) {
                exception = ex;
            }
            var childNode = new InspectorNode(prop.Name, node.Path, prop.PropertyType, propValue, node, InspectorNode.PropertyPrefix, getter.IsStatic, getter.IsPublic, getter.IsPrivate) {
                Exception = exception
            };
            node.Children.Add(childNode);
        }
    }
}
