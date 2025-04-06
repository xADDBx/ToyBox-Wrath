namespace ToyBox;
public abstract class FeatureWithPatch : ToggledFeature {
    protected Harmony HarmonyInstance = null!;
    protected virtual string HarmonyName => $"ToyBox.Feature.{Name}";
    public FeatureWithPatch() {
        HarmonyInstance = new(HarmonyName);
    }
    public void Patch() {
        if (IsEnabled) {
            HarmonyInstance.PatchCategory(HarmonyName);
        }
    }
    public void Unpatch() {
        HarmonyInstance.UnpatchAll(HarmonyName);
    }
    public override void Initialize() {
        Patch();
    }
    public override void Destroy() {
        Unpatch();
    }
}
