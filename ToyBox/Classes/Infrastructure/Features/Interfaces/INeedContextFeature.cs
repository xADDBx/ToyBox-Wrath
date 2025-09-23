namespace ToyBox;
public interface INeedContextFeature { }
public interface INeedContextFeature<T> : INeedContextFeature {
    public bool GetContext(out T? context);
}
