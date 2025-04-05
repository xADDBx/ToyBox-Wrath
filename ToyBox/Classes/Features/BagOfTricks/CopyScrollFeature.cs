using JetBrains.Annotations;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic;
using UnityEngine;
using Kingmaker.Blueprints.Items.Components;

namespace ToyBox.Classes.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Classes.Features.BagOfTricks.CopyScrollFeature")]
public partial class CopyScrollFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Classes.Features.BagOfTricks.CopyScrollFeature";
    public override bool IsEnabled => Settings.CanCopyScrolls;

    [LocalizedString("ToyBox_Classes_Features_BagOfTricks_CopyScrollFeature_CanCopyScrollsText", "Can Copy Scrolls")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Classes_Features_BagOfTricks_CopyScrollFeature_AllowSpontaneousCastersToCopyScr", "Allow spontaneous casters to copy scrolls into their spell books")]
    public override partial string Description { get; }
    public override void OnGui() {
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(Settings.CanCopyScrolls, Name.Cyan(), GUILayout.ExpandWidth(false));
            if (newValue != Settings.CanCopyScrolls) {
                Settings.CanCopyScrolls = newValue;
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
    [HarmonyPatch(typeof(CopyScroll), nameof(CopyScroll.CanCopySpell))]
    [HarmonyPatch(new Type[] { typeof(BlueprintAbility), typeof(Spellbook) })]
    [HarmonyPostfix]
    public static void CopyScrolls_Postfix([NotNull] BlueprintAbility spell, [NotNull] Spellbook spellbook, ref bool __result){
        if (spellbook.IsKnown(spell)) {
            __result = false;
            return;
        }
        var spellListContainsSpell = spellbook.Blueprint.SpellList.Contains(spell);

        if (spellbook.Blueprint.Spontaneous && spellListContainsSpell) {
            __result = true;
            return;
        }

        __result = spellbook.Blueprint.CanCopyScrolls && spellListContainsSpell;
    }
}
