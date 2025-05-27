using Kingmaker.Globalmap;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.InstantChangePartyMembersFeature")]
public partial class InstantChangePartyMembersFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.InstantChangePartyMembersFeature";
    public override ref bool IsEnabled => ref Settings.ToggleInstantChangePartyMembers;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_InstantChangePartyMembersFeature_Name", "Instant change party members")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_InstantChangePartyMembersFeature_Description", "Spend no time when switching out party members regardless of your location")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(GlobalMapPathManager), nameof(GlobalMapPathManager.GetTimeToCapital)), HarmonyPostfix]
    public static void GlobalMapPathManager_GetTimeToCapital_Patch(bool andBack, ref TimeSpan? __result) {
        if (andBack && __result != null) {
            __result = TimeSpan.Zero;
        }
    }
}
