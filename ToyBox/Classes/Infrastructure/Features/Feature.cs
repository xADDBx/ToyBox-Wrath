namespace ToyBox;
public abstract class Feature {
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual bool IsEnabled { get; } = false;
    public abstract void OnGui();
    public virtual string[] SortKeys {
        get {
            return [Name];
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
