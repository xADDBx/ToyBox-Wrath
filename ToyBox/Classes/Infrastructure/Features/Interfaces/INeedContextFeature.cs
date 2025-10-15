namespace ToyBox;
public interface INeedContextFeature { }
public interface INeedContextFeature<T> : INeedContextFeature {
    public bool GetContext(out T? context);
}
public interface INeedContextFeature<TIn, TOut> : INeedContextFeature {
    public bool GetContext(TIn? data, out TOut? context);
}
