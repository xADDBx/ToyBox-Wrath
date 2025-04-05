using JetBrains.Annotations;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic;
using UnityEngine;
using Kingmaker.Blueprints.Items.Components;

namespace ToyBox.Classes.Features.BagOfTricks;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Classes.Features.BagOfTricks.SpontaneousCasterCopyScrollFeature")]
public partial class SpontaneousCasterCopyScrollFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Classes.Features.BagOfTricks.SpontaneousCasterCopyScrollFeature";
    public override bool IsEnabled => Settings.SpontaneousCasterCanCopyScrolls;

    [LocalizedString("ToyBox_Classes_Features_BagOfTricks_SpontaneousCasterCopyScrollFeature_CanCopyScrollsText", "Spontaneous Caster Scroll Copy")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Classes_Features_BagOfTricks_SpontaneousCasterCopyScrollFeature_AllowSpontaneousCastersToCopyScr", "Allow spontaneous casters to copy scrolls into their spell books")]
    public override partial string Description { get; }
    public override void OnGui() {
        using (HorizontalScope()) {
            var newValue = GUILayout.Toggle(Settings.SpontaneousCasterCanCopyScrolls, Name.Cyan(), GUILayout.ExpandWidth(false));
            if (newValue != Settings.SpontaneousCasterCanCopyScrolls) {
                Settings.SpontaneousCasterCanCopyScrolls = newValue;
                if (newValue) {
                    Initialize();
                } else {
                    Destroy();
                }
            }
            GUILayout.Space(10);
            GUILayout.Label(Description.Green(), GUILayout.ExpandWidth(false));
        }
    }
    [HarmonyPatch(typeof(CopyScroll), nameof(CopyScroll.CanCopySpell), [typeof(BlueprintAbility), typeof(Spellbook)]), HarmonyPostfix]
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
