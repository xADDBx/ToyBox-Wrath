using Kingmaker.View.MapObjects.InteractionRestrictions;
using UnityEngine;

namespace ToyBox.Classes.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Classes.Features.BagOfTricks.ToggleLockJamFeature")]
public partial class ToggleLockJamFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Classes.Features.BagOfTricks.ToggleLockJamFeature";
    public override bool IsEnabled => Settings.ToggleLockJam;

    [LocalizedString("ToyBox_Classes_Features_BagOfTricks_ToggleLockJamFeature_ToggleLockJamText", "Toggle Lock Jam")]
    public override partial string Name { get; }

    [LocalizedString("ToyBox_Classes_Features_BagOfTricks_ToggleLockJamFeature_PreventsLocksFromJammingText", "Prevents Locks from jamming"
    )]
    public override partial string Description { get; }
    public override void OnGui() {
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(Settings.ToggleLockJam, Name.Cyan(), GUILayout.ExpandWidth(false));
            if (newValue != Settings.ToggleLockJam) {
                Settings.ToggleLockJam = newValue;
                if (newValue) {
                    Patch();
                } else {
                    Unpatch();
                }
            }
            GUILayout.Space(10);
            GUILayout.Label(Description.Green(), GUILayout.ExpandWidth(false));
        }
    }
    [HarmonyPatch(typeof(DisableDeviceRestrictionPart), nameof(DisableDeviceRestrictionPart.CheckRestriction)), HarmonyPostfix]
    public static void CheckRestriction_Patch(DisableDeviceRestrictionPart __instance) {
        __instance.Jammed = false; // still not doing a transpiler
    }
}
