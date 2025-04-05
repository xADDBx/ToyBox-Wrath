using System.Diagnostics;
using ToyBox.Infrastructure.UI;

namespace ToyBox;
public abstract class FeatureTab {
    private Dictionary<string, List<Feature>> FeatureGroups { get; set; } = new();
    public abstract string Name { get; }
    public virtual void AddFeature(Feature feature, string groupName = "") {
        if (feature is INeedEarlyInitFeature) {
            feature.Initialize();
        }
        if (!FeatureGroups.TryGetValue(groupName, out var group)) {
            group = new();
            FeatureGroups[groupName] = group;
        }
        group.Add(feature);
    }
    public IEnumerable<Feature> GetFeatures() {
        foreach (var group in FeatureGroups.Values) {
            foreach (var feature in group) {
                yield return feature;
            }
        }
    }
    public IEnumerable<(string groupName, List<Feature> features)> GetGroups() {
        foreach (var group in FeatureGroups) {
            yield return (group.Key, group.Value);
        }
    }
    public virtual void InitializeAll() {
        Stopwatch a = Stopwatch.StartNew();
        foreach (var feature in GetFeatures()) {
            if (feature is not INeedEarlyInitFeature) {
                feature.Initialize();
            }
        }
        Debug($"!!Threaded!!: {GetType().Name} lazy init took {a.ElapsedMilliseconds}ms");
    }
    public virtual void DestroyAll() {
        foreach (var feature in GetFeatures()) {
            feature.Destroy();
        }
    }
    public virtual void OnGui() {
        Div.DrawDiv();
        foreach (var feature in GetFeatures()) {
            feature.OnGui();
            Div.DrawDiv();
        }
    }
}
