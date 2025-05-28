using Kingmaker.UnitLogic.Abilities;
using ToyBox.Infrastructure;
using static UnityModManagerNet.UnityModManager.Param;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.IgnoreAllRequirementsForAbilitiesFeature")]
public partial class IgnoreAllRequirementsForAbilitiesFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.IgnoreAllRequirementsForAbilitiesFeature";
    public override ref bool IsEnabled => ref Settings.ToggleIgnoreAllRequirementsForAbilities;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_IgnoreAllRequirementsForAbilitiesFeature_Name", "Ignore all Requirements for Abilities")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_IgnoreAllRequirementsForAbilitiesFeature_Description", "Allows casting any abilities regardless of requirements")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.CanBeCastByCaster), MethodType.Getter), HarmonyPostfix]
    public static void AbilityData_CanBeCastByCaster_Patch(ref bool __result, AbilityData __instance) {
        if (ToyBoxUnitHelper.IsPartyOrPet(__instance?.Caster?.Unit)) {
            __result = true;
        }
    }
}
