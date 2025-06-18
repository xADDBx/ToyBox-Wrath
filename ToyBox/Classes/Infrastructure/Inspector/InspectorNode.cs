using Kingmaker.Blueprints;
using System.Collections;
using ToyBox.Infrastructure.Utilities;
using UnityEngine;

namespace ToyBox.Infrastructure.Inspector;
public class InspectorNode : IComparable {
    private static readonly HashSet<string> m_ContainerMembers = [GameObjectChildPrefix, GameObjectComponentPrefix, EnumerableItemPrefix];
    public const string GameObjectChildPrefix = "ci";
    public const string GameObjectComponentPrefix = "co";
    public const string EnumerableItemPrefix = "i";
    public const string FieldPrefix = "f";
    public const string PropertyPrefix = "p";

    private readonly string m_Name;
    private readonly Type m_FieldType;
    private readonly string m_ContainerPrefix;
    internal object? Value;
    public readonly string Path;
    public readonly Type ConcreteType;
    public readonly InspectorNode? Parent;
    public readonly bool IsStatic = false;
    public readonly bool IsPublic = false;
    public readonly bool IsPrivate = false;
    public readonly bool IsNullable = false;
    public readonly bool IsEnumerable = false;
    public readonly bool IsGameObject = false;
    public readonly bool IsNull = false;
    public bool IsExpanded = false;
    public Exception? Exception;
    public List<InspectorNode>? Children;
    public Color? ColorOverride;
    public IEnumerable AsEnumerableSafe() {
        if (Value is IEnumerable enumerable) {
            return enumerable;
        } else {
            return Array.Empty<object>();
        }
    }
    public int? ElementCount {
        get {
            field ??= Children?.Where(x => m_ContainerMembers.Contains(x.m_ContainerPrefix))?.Count() ?? 0;
            return field;
        }
    }
    public string ValueText {
        get {
            field ??= GetValueText();
            return field;
        }
    }
    public string NameText {
        get {
            field ??= GetNameText();
            return field;
        }
    }
    public string AfterText {
        get {
            field ??= GetAfterText();
            return field;
        }
    }
    private string GetAfterText() {
        if (ConcreteType != m_FieldType) {
            return ToyBoxReflectionHelper.GetNameWithGenericsResolved(ConcreteType).Yellow();
        } else {
            return "";
        }
    }
    private string GetValueText() {
        var valueText = "";
        if (Exception != null) {
            valueText = "<exception>";
            ColorOverride = Color.red;
        } else {
            if (IsNull) {
                valueText = "<null>";
                ColorOverride = Color.gray;
            } else {
                try {
                    valueText = Value!.ToString();
                } catch (Exception ex) {
                    Exception = ex;
                    valueText = "<exception>";
                    ColorOverride = Color.red;
                }
            }
        }
        return valueText;
    }
    private string GetNameText() {
        string nameText = $"[{m_ContainerPrefix}] ".Grey();
        if (IsStatic) {
            nameText += "[s] ".Magenta();
        }
        nameText += m_Name;

        if (IsGameObject || IsEnumerable) {
            nameText += " " + $"[{ElementCount}]".Yellow();
        }
        string typeName = ToyBoxReflectionHelper.GetNameWithGenericsResolved(m_FieldType);
        if (ToyBoxReflectionHelper.PrimitiveTypes.Contains(ConcreteType)) {
            typeName = typeName.Grey();
        } else if (IsGameObject) {
            typeName = typeName.Magenta();
        } else if (IsEnumerable) {
            typeName = typeName.Cyan();
        } else {
            typeName = typeName.Orange();
        }
        nameText += " : " + typeName;
        return nameText;
    }
    public InspectorNode(string name, string path, Type type, object? value, InspectorNode? parent, string containerPrefix, bool isStatic = false, bool isPublic = true, bool isPrivate = false) {
        m_Name = name;
        Value = value;
        Path = path + name + "/";
        if (Value is BlueprintReferenceBase bpRef) {
            bpRef.GetBlueprint();
        }
        m_FieldType = type;
        if (Value != null) {
            type = Value.GetType();
        }
        Parent = parent;
        IsStatic = isStatic;
        IsPublic = isPublic;
        IsPrivate = isPrivate;
        IsNull = value is null;
        if (ToyBoxReflectionHelper.IsNullableT(type, out var maybeUnderlying)) {
            ConcreteType = maybeUnderlying!;
            IsNullable = true;
        } else {
            ConcreteType = type;
            IsNullable = false;
        }
        IsGameObject = typeof(GameObject).IsAssignableFrom(ConcreteType);
        IsEnumerable = typeof(IEnumerable).IsAssignableFrom(ConcreteType) && ConcreteType != typeof(string);
        m_ContainerPrefix = containerPrefix;
    }

    public int CompareTo(object obj) {
        var other = obj as InspectorNode;
        if (other == null) {
            return 1;
        }
        return (m_ContainerPrefix + m_Name).CompareTo(other.m_ContainerPrefix + other.m_Name);
    }
}
