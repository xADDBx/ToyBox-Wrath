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
    public string Name;
    public string Path;
    public Type FieldType;
    public Type ConcreteType;
    public object? Value;
    public InspectorNode? Parent;
    public bool IsExpanded = false;
    public bool IsStatic = false;
    public bool IsPublic = false;
    public bool IsPrivate = false;
    public bool IsNullable = false;
    public bool IsEnumerable = false;
    public bool IsGameObject = false;
    public Exception? Exception;
    public List<InspectorNode>? Children;
    public string ContainerPrefix;
    public int? ElementCount {
        get {
            field ??= Children?.Where(x => m_ContainerMembers.Contains(x.ContainerPrefix))?.Count() ?? 0;
            return field;
        }
    }

    public InspectorNode(string name, string path, Type type, object? value, InspectorNode? parent, string containerPrefix, bool isStatic = false, bool isPublic = true, bool isPrivate = false) {
        Name = name;
        Value = value;
        Path = path + name + "/";
        if (Value is BlueprintReferenceBase bpRef) {
            bpRef.GetBlueprint();
        }
        FieldType = type;
        if (Value != null) {
            type = Value.GetType();
        }
        Parent = parent;
        IsStatic = isStatic;
        IsPublic = isPublic;
        IsPrivate = isPrivate;
        if (ToyBoxReflectionHelper.IsNullableT(type, out var maybeUnderlying)) {
            ConcreteType = maybeUnderlying!;
            IsNullable = true;
        } else {
            ConcreteType = type;
            IsNullable = false;
        }
        IsGameObject = typeof(GameObject).IsAssignableFrom(ConcreteType);
        IsEnumerable = typeof(IEnumerable).IsAssignableFrom(ConcreteType) && ConcreteType != typeof(string);
        ContainerPrefix = containerPrefix;
    }

    public int CompareTo(object obj) => Name.CompareTo((obj as InspectorNode)?.Name ?? "");
}
