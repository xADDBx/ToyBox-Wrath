namespace ToyBox;
public abstract class Feature {
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
