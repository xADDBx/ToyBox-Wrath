namespace ToyBox;
public abstract class FeatureTab {
    internal List<Feature> Features { get; set; } = new();
    public abstract string Name { get; }
    public virtual void PatchAll() {
        foreach (var feature in Features.OfType<FeatureWithPatch>()) {
            feature.Patch();
        }
    }
    public virtual void UnpatchAll() {
        foreach (var feature in Features.OfType<FeatureWithPatch>()) {
            feature.Unpatch();
        }
    }
    public virtual void OnGui() {
        foreach (var feature in Features) {
            feature.OnGui();
        }
    }
}
