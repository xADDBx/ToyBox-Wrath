using Kingmaker.EntitySystem.Stats;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.UnlimitedModifierStackingFeature")]
public partial class UnlimitedModifierStackingFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.UnlimitedModifierStackingFeature";
    public override ref bool IsEnabled => ref Settings.ToggleUnlimitedModifierStacking;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_UnlimitedModifierStackingFeature_Name", "Unlimited Stacking of Modifiers")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_UnlimitedModifierStackingFeature_Description", "Forces any kind of (Stat/AC/Hit/Etc) bonus to stack, even if they would normally override each other")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(ModifiableValue.Modifier), nameof(ModifiableValue.Modifier.Stacks), MethodType.Getter), HarmonyPrefix]
    public static void ModifiableValue_UpdateValue_Patch(ref ModifiableValue.Modifier __instance) {
        __instance.StackMode = ModifiableValue.StackMode.ForceStack;
    }
}
