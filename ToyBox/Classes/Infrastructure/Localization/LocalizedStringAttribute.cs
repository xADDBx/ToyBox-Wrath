namespace ToyBox;
[AttributeUsage(AttributeTargets.Field)]
public class LocalizedStringAttribute : Attribute {
    public string Key { get; }
    public LocalizedStringAttribute(string key) {
        Key = key;
    }
}
