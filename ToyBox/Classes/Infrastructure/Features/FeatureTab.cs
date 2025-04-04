using System.Diagnostics;
using ToyBox.Infrastructure.UI;

namespace ToyBox;
public abstract class FeatureTab {
    private List<Feature> Features { get; set; } = new();
    public abstract string Name { get; }
    public virtual void AddFeature(Feature feature) {
        if (feature is INeedEarlyInitFeature) {
            feature.Initialize();
        }
        Features.Add(feature);
    }
    public IEnumerable<Feature> GetFeatures() => Features;
    public virtual void InitializeAll() {
        Stopwatch a = Stopwatch.StartNew();
        foreach (var feature in Features) {
            if (feature is not INeedEarlyInitFeature) {
                feature.Initialize();
            }
        }
        Debug($"!!Threaded!!: {GetType().Name} lazy init took {a.ElapsedMilliseconds}ms");
    }
    public virtual void DestroyAll() {
        foreach (var feature in Features) {
            feature.Destroy();
        }
    }
    public virtual void OnGui() {
        Div.DrawDiv();
        foreach (var feature in Features) {
            feature.OnGui();
            Div.DrawDiv();
        }
    }
}
