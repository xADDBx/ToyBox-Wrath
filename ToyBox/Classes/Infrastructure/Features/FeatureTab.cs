namespace ToyBox;
public abstract class FeatureTab {
    internal List<Feature> Features { get; set; } = new();
    public abstract string Name { get; }
    public virtual void InitializeAll() {
        foreach (var feature in Features) {
            feature.Initialize();
        }
    }
    public virtual void DestroyAll() {
        foreach (var feature in Features) {
            feature.Destroy();
        }
    }
    public virtual void OnGui() {
        foreach (var feature in Features) {
            feature.OnGui();
        }
    }
}
