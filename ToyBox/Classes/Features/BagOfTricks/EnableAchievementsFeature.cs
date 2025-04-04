using Kingmaker.Achievements;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints;
using Kingmaker.Settings;
using Kingmaker;
using UnityEngine;
using System.Reflection.Emit;
using Kingmaker.Modding;

namespace ToyBox.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.EnableAchievementsFeature")]
public partial class EnableAchievementsFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.EnableAchievementsFeature";
    public override bool IsEnabled => Settings.EnableModdedAchievements;

    [LocalizedString("ToyBox_Features_BagOfTricks_EnableAchievementsFeature_AllowAchievementsWhileUsingModsT", "Allow Achievements While Using Mods")]
    public override partial string Name { get; }

    [LocalizedString("ToyBox_Features_BagOfTricks_EnableAchievementsFeature_ThisIsIntendedForYouToBeAbleToEn", "This is intended for you to be able to enjoy the game while using mods that enhance your quality of life.")]
    public override partial string Description { get; }
    public override void OnGui() {
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(Settings.EnableModdedAchievements, Name.Cyan(), GUILayout.ExpandWidth(false));
            if (newValue != Settings.EnableModdedAchievements) {
                Settings.EnableModdedAchievements = newValue;
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
    [HarmonyPatch(typeof(AchievementEntity), nameof(AchievementEntity.IsDisabled), MethodType.Getter), HarmonyTranspiler, HarmonyDebug]
    private static IEnumerable<CodeInstruction> AchievementEntity_IsDisabled_Patch(IEnumerable<CodeInstruction> instructions) {
        foreach (var instruction in instructions) {
            if (instruction.Calls(AccessTools.PropertyGetter(typeof(Player), nameof(Player.ModsUser)))) {
                yield return new(OpCodes.Pop);
                yield return new(OpCodes.Ldc_I4_0);
            } else if (instruction.Calls(AccessTools.PropertyGetter(typeof(OwlcatModificationsManager), nameof(OwlcatModificationsManager.IsAnyModActive)))) {
                yield return new(OpCodes.Pop);
                yield return new(OpCodes.Ldc_I4_0);
            } else {
                yield return instruction;
            }
        }
    }
    [HarmonyPatch(typeof(Player), nameof(Player.ModsUser), MethodType.Setter), HarmonyPrefix]
    public static void Player_ModsUser_Patch(ref bool value) {
        value = false;
    }
}
