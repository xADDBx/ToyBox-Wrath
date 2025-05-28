using Kingmaker;
using Kingmaker.Blueprints.Items.Armors;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.DisableArmourMaxDexterityFeature")]
public partial class DisableArmourMaxDexterityFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.DisableArmourMaxDexterityFeature";
    public override ref bool IsEnabled => ref Settings.ToggleDisableArmourMaxDexterity;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableArmourMaxDexterityFeature_Name", "Disable Armor Max Dexterity")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_DisableArmourMaxDexterityFeature_Description", "Some armours have a limit to their dexterity bonus. This feature disables said limit for every armour.")]
    public override partial string Description { get; }
    public override void Initialize() {
        base.Initialize();
    }
    public override void Destroy() {
        base.Destroy();
    }
    public static void Update() {
        var party = Game.Instance?.Player?.PartyAndPets;
        if (party != null) {
            foreach (var member in party) {
                var body = member?.Descriptor?.Body;
                if (body != null) {
                    body.Armor?.MaybeArmor?.RecalculateStats();
                    body.SecondaryHand?.MaybeShield?.ArmorComponent?.RecalculateStats();
                }
            }
        }
    }
    [HarmonyPatch(typeof(BlueprintArmorType), nameof(BlueprintArmorType.HasDexterityBonusLimit), MethodType.Getter), HarmonyPostfix]
    private static void BlueprintArmorType_HasDexterityBonusLimit_Patch(ref bool __result) {
        __result = false;
    }
}
