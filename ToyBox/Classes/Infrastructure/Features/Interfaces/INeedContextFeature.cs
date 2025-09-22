namespace ToyBox;
public interface INeedContextFeature { }
public interface INeedContextFeature<T> : INeedContextFeature {
    public abstract void OnGui(T context);
}
