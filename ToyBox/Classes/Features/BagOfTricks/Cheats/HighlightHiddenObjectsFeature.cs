using Kingmaker;
using Kingmaker.EntitySystem;
using Kingmaker.View.MapObjects;
using System.Reflection.Emit;

namespace ToyBox.Features.BagOfTricks.Cheats;

[HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.Cheats.HighlightHiddenObjectsFeature")]
public partial class HighlightHiddenObjectsFeature : FeatureWithPatch {
    protected override string HarmonyName => "ToyBox.Features.BagOfTricks.Cheats.HighlightHiddenObjectsFeature";
    public override ref bool IsEnabled => ref Settings.HighlightHiddenObjects;
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_HighlightHiddenObjectsFeature_Name", "Highlight Hidden Objects")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_HighlightHiddenObjectsFeature_Description", "Highlights objects even if they would normally be hidden by a perception Check or otherwise. Optionally also highlight objects in Fog Of War and hidden traps.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_HighlightHiddenObjectsFeature_AlsoHighlightHiddenTrapsText", "Also Highlight Hidden Traps")]
    private static partial string AlsoHighlightHiddenTrapsText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_HighlightHiddenObjectsFeature_AlsoHighlightInFogOfWarText", "Also Highlight in Fog of War")]
    private static partial string AlsoHighlightInFogOfWarText { get; }
    public override void Initialize() {
        base.Initialize();
        foreach (var mapObjectEntityData in Game.Instance.State.MapObjects) {
            mapObjectEntityData.View.UpdateHighlight();
        }
    }
    public override void Destroy() {
        base.Destroy();
        foreach (var mapObjectEntityData in Game.Instance.State.MapObjects) {
            mapObjectEntityData.View.UpdateHighlight();
        }
    }
    public override void OnGui() {
        using (VerticalScope()) {
            UI.Toggle(Name, Description, ref Settings.HighlightHiddenObjects, Initialize, Destroy);
            if (Settings.HighlightHiddenObjects) {
                using (HorizontalScope()) {
                    Space(50);
                    UI.Toggle(AlsoHighlightHiddenTrapsText, "", ref Settings.HighlightHiddenTraps, Initialize, Destroy);
                }
                using (HorizontalScope()) {
                    Space(50);
                    UI.Toggle(AlsoHighlightInFogOfWarText, "", ref Settings.HighlightInFogOfWar, Initialize, Destroy);
                }
            }
        }
    }
    [HarmonyPatch(typeof(MapObjectView), nameof(MapObjectView.ShouldBeHighlighted)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> MapObjectView_ShouldBeHighlighted_Patch(IEnumerable<CodeInstruction> instructions) {
        var get_IsInFogOfWar = AccessTools.PropertyGetter(typeof(EntityDataBase), nameof(EntityDataBase.IsInFogOfWar));
        var get_IsRevealed = AccessTools.PropertyGetter(typeof(EntityDataBase), nameof(EntityDataBase.IsRevealed));
        var get_IsPerceptionCheckPassed = AccessTools.PropertyGetter(typeof(StaticEntityData), nameof(StaticEntityData.IsPerceptionCheckPassed));
        foreach (var inst in instructions) {
            if (inst.Calls(get_IsRevealed) || inst.Calls(get_IsPerceptionCheckPassed)) {
                var popInst = new CodeInstruction(OpCodes.Pop).MoveLabelsFrom(inst);
                yield return popInst;
                yield return new(OpCodes.Ldc_I4_1);
            } else if (Settings.HighlightInFogOfWar && inst.Calls(get_IsInFogOfWar)) {
                var popInst = new CodeInstruction(OpCodes.Pop).MoveLabelsFrom(inst);
                yield return popInst;
                yield return new(OpCodes.Ldc_I4_0);
            } else {
                yield return inst;
            }
        }
    }
    [HarmonyPatch, HarmonyPatchCategory("ToyBox.Features.BagOfTricks.Cheats.HighlightHiddenObjectsFeature")]
    private static class HighlightHiddenTraps_Patch {
        [HarmonyPrepare]
        private static bool ShouldRun() => Settings.HighlightHiddenTraps;
        [HarmonyPatch(typeof(InteractionPart), nameof(InteractionPart.HasVisibleTrap)), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> InteractionPart_HasVisibleTrap_Patch(IEnumerable<CodeInstruction> instructions) {
            var get_IsPerceptionCheckPassed = AccessTools.PropertyGetter(typeof(StaticEntityData), nameof(StaticEntityData.IsPerceptionCheckPassed));
            foreach (var inst in instructions) {
                if (inst.Calls(get_IsPerceptionCheckPassed)) {
                    var popInst = new CodeInstruction(OpCodes.Pop).MoveLabelsFrom(inst);
                    yield return popInst;
                    yield return new(OpCodes.Ldc_I4_1);
                } else {
                    yield return inst;
                }
            }
        }
    }
}
