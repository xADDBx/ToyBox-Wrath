using System.Reflection;

namespace ToyBox;
[AttributeUsage(AttributeTargets.Field)]
public class LocalizedStringAttribute : Attribute {
    private static IEnumerable<(FieldInfo, LocalizedStringAttribute)>? _fieldsWithAttribute;
    public string Key { get; }
    public LocalizedStringAttribute(string key) {
        Key = key;
    }
    public static IEnumerable<(FieldInfo, LocalizedStringAttribute)> GetFieldsWithAttribute() {
        _fieldsWithAttribute ??= Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
        .Select(f => (f, f.GetCustomAttribute<LocalizedStringAttribute>())).Where(pair => pair.Item2 != null);
        return _fieldsWithAttribute;
    }
}
