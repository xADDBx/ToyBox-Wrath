using Kingmaker.View.MapObjects.InteractionRestrictions;
using System.Reflection.Emit;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.Cheats.ToggleLockJamFeature")]
public partial class ToggleLockJamFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.ToggleLockJamFeature";
    public override ref bool IsEnabled => ref Settings.ToggleLockJam;

    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_ToggleLockJamFeature_ToggleLockJamText", "Toggle Lock Jam")]
    public override partial string Name { get; }

    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_ToggleLockJamFeature_PreventsLocksFromJammingText", "Prevents Locks from jamming")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(DisableDeviceRestrictionPart), nameof(DisableDeviceRestrictionPart.CheckRestriction)), HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> CheckRestriction_Patch(IEnumerable<CodeInstruction> instructions) {
        var jammedField = AccessTools.Field(typeof(DisableDeviceRestrictionPart), nameof(DisableDeviceRestrictionPart.Jammed));
        foreach (var instruction in instructions) {
            if (instruction.LoadsField(jammedField)) {
                yield return new(OpCodes.Pop);
                yield return new(OpCodes.Ldc_I4_0);
            } else if (instruction.StoresField(jammedField)) {
                yield return new(OpCodes.Pop);
                yield return new(OpCodes.Ldc_I4_0);
                yield return instruction;
            } else {
                yield return instruction;
            }
        }
    }
}
