namespace ToyBox;
public abstract class FeatureWithPatch : Feature {
    protected Harmony HarmonyInstance = null!;
    protected virtual string HarmonyName => $"ToyBox.Feature.{Name}";
    public FeatureWithPatch() {
        HarmonyInstance = new(HarmonyName);
    }
    public virtual void Patch() {
        if (IsEnabled) {
            HarmonyInstance.PatchCategory(HarmonyName);
        }
    }
    public virtual void Unpatch() {
        HarmonyInstance.UnpatchAll(HarmonyName);
    }
}
