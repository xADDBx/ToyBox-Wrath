using System.Collections.Concurrent;

namespace ToyBox;
public abstract class Feature {
    private static readonly ConcurrentDictionary<Type, Feature> m_Instances = [];
    protected Feature() {
        var t = GetType();
        if (!m_Instances.TryAdd(t, this)) {
            throw new InvalidOperationException($"Feature of type {t.Name} was already constructed.");
        }
    }
    public static Feature GetInstance(Type featureType) {
        if (!m_Instances.TryGetValue(featureType, out var inst)) {
            inst = (Feature)Activator.CreateInstance(featureType, true);
        }
        return m_Instances[featureType] = inst;
    }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract void OnGui();
    public virtual void Initialize() { }
    public virtual void Destroy() { }
    public virtual bool ShouldHide => false;
    public virtual string SortKey {
        get {
            return Name;
        }
        set { }
    }
    public virtual string SearchKey {
        get {
            return $"{Name} {Description}";
        }
        set { }
    }
}
