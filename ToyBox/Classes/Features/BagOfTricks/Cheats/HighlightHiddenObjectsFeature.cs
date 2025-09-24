using Kingmaker;
using Kingmaker.EntitySystem;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.View.MapObjects.Traps;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Highlighting;
using System.Reflection.Emit;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks.Cheats;

[NeedsTesting]
[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Cheats.HighlightHiddenObjectsFeature")]
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
    private const string ObjName = "ToyBox.HiddenHighlighter";
    private const string DecalName = "ToyBox.DecalHiddenHighlighter";
    private static readonly Color HighlightColor0 = new(1.0f, 0.0f, 1.0f, 0.8f);
    private static readonly Color HighlightColor1 = new(0.0f, 0.0f, 1.0f, 1.0f);
    public override void Initialize() {
        base.Initialize();
        Main.ScheduleForMainThread(() => {
            if (Game.Instance?.State != null) {
                foreach (var mapObjectEntityData in Game.Instance.State.MapObjects) {
                    mapObjectEntityData.View.UpdateHighlight();
                }
            }
        });
    }
    public override void Destroy() {
        base.Destroy();
        Main.ScheduleForMainThread(() => {
            if (Game.Instance?.State != null) {
                foreach (var mapObjectEntityData in Game.Instance.State.MapObjects) {
                    try {
                        var view = mapObjectEntityData.View;
                        HighlightDestroy(view);
                        view.UpdateHighlight();
                    } catch (Exception ex) {
                        Trace(ex.ToString());
                    }
                }
            }
        });
    }
    public void Reinitialize() {
        Destroy();
        Initialize();
    }
    public override void OnGui() {
        using (VerticalScope()) {
            UI.Toggle(Name, Description, ref Settings.HighlightHiddenObjects, Initialize, Destroy);
            if (Settings.HighlightHiddenObjects) {
                using (HorizontalScope()) {
                    Space(50);
                    UI.Toggle(AlsoHighlightHiddenTrapsText, "", ref Settings.HighlightHiddenTraps, Initialize, Reinitialize);
                }
                using (HorizontalScope()) {
                    Space(50);
                    UI.Toggle(AlsoHighlightInFogOfWarText, "", ref Settings.HighlightInFogOfWar, Initialize, Reinitialize);
                }
            }
        }
    }
    [HarmonyPatch(typeof(MapObjectView), nameof(MapObjectView.ShouldBeHighlighted)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> MapObjectView_ShouldBeHighlighted_Patch(IEnumerable<CodeInstruction> instructions) {
        var get_IsInFogOfWar = AccessTools.PropertyGetter(typeof(EntityDataBase), nameof(EntityDataBase.IsInFogOfWar));
        var get_IsRevealed = AccessTools.PropertyGetter(typeof(EntityDataBase), nameof(EntityDataBase.IsRevealed));
        var get_IsPerceptionCheckPassed = AccessTools.PropertyGetter(typeof(StaticEntityData), nameof(StaticEntityData.IsPerceptionCheckPassed));
        var get_HighlightOnHover = AccessTools.PropertyGetter(typeof(MapObjectView), nameof(MapObjectView.HighlightOnHover));
        foreach (var inst in instructions) {
            if (inst.Calls(get_IsRevealed) || inst.Calls(get_IsPerceptionCheckPassed)) {
                var popInst = new CodeInstruction(OpCodes.Pop).MoveLabelsFrom(inst);
                yield return popInst;
                yield return new(OpCodes.Ldc_I4_1);
            } else if (Settings.HighlightInFogOfWar && inst.Calls(get_IsInFogOfWar)) {
                var popInst = new CodeInstruction(OpCodes.Pop).MoveLabelsFrom(inst);
                yield return popInst;
                yield return new(OpCodes.Ldc_I4_0);
            } else if (inst.Calls(get_HighlightOnHover)) {
                yield return CodeInstruction.Call((MapObjectView view) => ShouldHighlightOnHover(view)).WithLabels(inst.ExtractLabels());
            } else {
                yield return inst;
            }
        }
    }
    private static bool ShouldHighlightOnHover(MapObjectView view) {
        if (view.GetComponent<PerceptionCheckComponent>() != null) {
            if (!Settings.HighlightHiddenTraps) {
                if (view.Data.Parts.GetAll<InteractionPart>().Any(part => part.Settings.Trap != null) || view is TrapObjectView) {
                    return view.HighlightOnHover;
                }
            }
            return true;
        } else {
            return view.HighlightOnHover;
        }
    }
    [HarmonyPatch(typeof(MapObjectView), nameof(MapObjectView.UpdateHighlight)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> MapObjectView_UpdateHighlight_Patch(IEnumerable<CodeInstruction> instructions) {
        var shouldBeHighlighted = AccessTools.Method(typeof(MapObjectView), nameof(MapObjectView.ShouldBeHighlighted));
        foreach (var inst in instructions) {
            yield return inst;
            if (inst.Calls(shouldBeHighlighted)) {
                yield return new(OpCodes.Dup);
                yield return new(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call((bool shouldBeHighlighted, MapObjectView view) => UpdateHighlight_Patch(shouldBeHighlighted, view));
            }
        }
    }
    private static void UpdateHighlight_Patch(bool shouldBeHighlighted, MapObjectView view) {
        if (view.GetComponent<PerceptionCheckComponent>() != null) {
            if (!Settings.HighlightHiddenTraps) {
                if (view.Data.Parts.GetAll<InteractionPart>().Any(part => part.Settings.Trap != null) || view is TrapObjectView) {
                    return;
                }
            }
            if (view.Data.IsPerceptionCheckPassed) {
                HighlightDestroy(view);
            } else {
                if (shouldBeHighlighted) {
                    HighlightOn(view);
                } else {
                    HighlightOff(view);
                }
            }
        }
    }
    private static void HighlightDestroy(MapObjectView view) {
        var obj = view.transform.Find(ObjName)?.gameObject;
        if (obj != null) {
            UnityEngine.Object.Destroy(obj);
        } else {
            return;
        }

        var decal = view.transform.Find(DecalName)?.gameObject;
        if (decal != null) {
            UnityEngine.Object.Destroy(decal);
        }
    }
    private static void HighlightCreate(MapObjectView view) {
        if (view.transform.Find(ObjName)) return;
        var obj = new GameObject(ObjName);
        obj.transform.parent = view.transform;
        var highlighter = obj.AddComponent<Highlighter>();

        foreach (var polygon in view.transform.GetComponentsInChildren<ScriptZonePolygon>()) {
            var renderer = polygon.DecalMeshObject?.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            var decal = UnityEngine.Object.Instantiate(renderer.gameObject, renderer.transform.parent);
            decal.name = DecalName;

            var decal_renderer = decal.GetComponent<MeshRenderer>();
            decal_renderer.enabled = false;
            decal_renderer.forceRenderingOff = true;
        }

        foreach (var renderer in view.transform.GetComponentsInChildren<Renderer>()) {
            highlighter.AddExtraRenderer(renderer);
        }
    }
    private static void HighlightOn(MapObjectView view) {
        var obj = view.transform.Find(ObjName)?.gameObject;
        if (obj == null) {
            HighlightCreate(view);
            obj = view.transform.Find(ObjName)?.gameObject;
        }

        var highlighter = obj?.GetComponent<Highlighter>();
        if (highlighter != null) {
            highlighter.ConstantOnImmediate(HighlightColor0);
            highlighter.FlashingOn(HighlightColor0, HighlightColor1, 1.0f);

            var renderer = view.transform.Find(DecalName)?.gameObject?.GetComponent<MeshRenderer>();
            if (renderer == null) return;
            renderer.enabled = true;
            renderer.forceRenderingOff = true;
        }
    }
    private static void HighlightOff(MapObjectView view) {
        var obj = view.transform.Find(ObjName)?.gameObject;
        if (obj == null) return;

        var highlighter = obj.GetComponent<Highlighter>();
        if (highlighter != null) {
            highlighter.ConstantOff(0.0f);
            highlighter.FlashingOff();

            var renderer = view.transform.Find(DecalName)?.gameObject?.GetComponent<MeshRenderer>();
            if (renderer == null) return;
            renderer.enabled = false;
            renderer.forceRenderingOff = true;
        }
    }
    [HarmonyPatch]
    private static class HighlightHiddenTraps_Patch {
        [HarmonyPrepare]
        private static bool ShouldRun() => Settings.HighlightHiddenTraps;
        [HarmonyPatch(typeof(InteractionPart), nameof(InteractionPart.HasVisibleTrap)), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> InteractionPart_HasVisibleTrap_Patch(IEnumerable<CodeInstruction> instructions) {
            var get_IsPerceptionCheckPassed = AccessTools.PropertyGetter(typeof(StaticEntityData), nameof(StaticEntityData.IsPerceptionCheckPassed));
            foreach (var inst in instructions) {
                if (inst.Calls(get_IsPerceptionCheckPassed)) {
                    var popInst = new CodeInstruction(OpCodes.Pop);
                    yield return popInst.WithLabels(inst.ExtractLabels());
                    yield return new(OpCodes.Ldc_I4_1);
                } else {
                    yield return inst;
                }
            }
        }
    }
}
