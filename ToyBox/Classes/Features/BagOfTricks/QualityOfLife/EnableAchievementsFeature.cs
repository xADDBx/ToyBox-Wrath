using Kingmaker.Achievements;
using Kingmaker;
using System.Reflection.Emit;
using Kingmaker.Modding;

namespace ToyBox.Features.BagOfTricks.QoL;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.QoL.EnableAchievementsFeature")]
public partial class EnableAchievementsFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.QoL.EnableAchievementsFeature";
    public override ref bool IsEnabled => ref Settings.EnableModdedAchievements;

    [LocalizedString("ToyBox_Features_BagOfTricks_QoL_EnableAchievementsFeature_AllowAchievementsWhileUsingModsT", "Allow Achievements While Using Mods")]
    public override partial string Name { get; }

    [LocalizedString("ToyBox_Features_BagOfTricks_QoL_EnableAchievementsFeature_ThisIsIntendedForYouToBeAbleToEn", "This is intended for you to be able to enjoy the game while using mods that enhance your quality of life.")]
    public override partial string Description { get; }
    [HarmonyPatch(typeof(AchievementEntity), nameof(AchievementEntity.IsDisabled), MethodType.Getter), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> AchievementEntity_IsDisabled_Transpiler(IEnumerable<CodeInstruction> instructions) {
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
    public static void Player_ModsUser_Prefix(ref bool value) {
        value = false;
    }
}
