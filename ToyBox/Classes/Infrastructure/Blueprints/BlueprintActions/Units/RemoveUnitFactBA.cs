using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class RemoveUnitFactBA : BlueprintActionFeature, IBlueprintAction<BlueprintUnitFact>, INeedContextFeature<UnitEntityData> {
    private bool CanExecute(BlueprintUnitFact blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return unit.GetFact(blueprint) != null;
        }
        return false;
    }
    private bool CanExecuteSpell(BlueprintUnitFact blueprint, out Spellbook? sb, params object[] parameter) {
        sb = null;
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit && blueprint is BlueprintAbility ability && ability.IsSpell) {
            foreach (var spellbook in unit.Spellbooks) {
                if (spellbook.GetAllKnownSpells().Any(spell => spell.Blueprint == blueprint)) {
                    sb = spellbook;
                    return true;
                }
            }
        }
        return false;
    }
    private bool Execute(BlueprintUnitFact blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        var unit = (UnitEntityData)parameter[0];
        if (blueprint is BlueprintFeature feature) {
            foreach (var selection in unit.Progression.Selections) {
                foreach (var byLevel in selection.Value.SelectionsByLevel.ToList()) {
                    if (byLevel.Value.Contains(blueprint)) {
                        selection.Value.RemoveSelection(byLevel.Key, feature);
                        if (byLevel.Value.Count == 0) {
                            selection.Value.RemoveLevel(byLevel.Key);
                        }
                    }
                    if (selection.Value.SelectionsByLevel.Count == 0) {
                        Execute(selection.Key, unit);
                    }
                }
            }
            // if (feature is IFeatureSelection iFeatureSelection) {
            if (feature is BlueprintFeatureSelection blueprintFeatureSelection) {
                if (unit.Progression.Selections.TryGetValue(blueprintFeatureSelection, out var data)) {
                    foreach (var sel in data.SelectionsByLevel) {
                        foreach (var feat in sel.Value.ToArray()) {
                            Execute(feat, unit);
                        }
                        data.RemoveLevel(sel.Key);
                    }
                    unit.Progression.Selections.Remove(blueprintFeatureSelection);
                }
            }
            // }
        }
        unit.RemoveFact(blueprint);
        return true;
    }
    private bool ExecuteSpell(BlueprintUnitFact blueprint, Spellbook spellbook, params object[] parameter) {
        LogExecution(blueprint, spellbook, parameter);
        spellbook.RemoveSpell(blueprint as BlueprintAbility);
        return true;
    }
    public bool? OnGui(BlueprintUnitFact blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        bool canRemoveFact = CanExecute(blueprint, parameter);
        bool canRemoveSpell = CanExecuteSpell(blueprint, out var spellbook, parameter);
        if (canRemoveFact || canRemoveSpell) {
            if (canRemoveFact) {
                UI.Button(StyleActionString(RemoveText, isFeatureSearch), () => {
                    result = Execute(blueprint, parameter);
                });
                UI.Label(" ");
            }
            Space(10);
            if (canRemoveSpell) {
                UI.Button(StyleActionString(RemoveSpellText, isFeatureSearch), () => {
                    result = ExecuteSpell(blueprint, spellbook!, parameter);
                });
            }
        } else if (isFeatureSearch) {
            UI.Label(UnitDoesNotHaveThisFactText.Red().Bold());
        }
        return result;
    }
    public bool GetContext(out BlueprintUnitFact? context) => ContextProvider.Blueprint(out context);
    public bool GetContext(out UnitEntityData? context) => ContextProvider.UnitEntityData(out context);
    public override void OnGui() {
        if (GetContext(out BlueprintUnitFact? bp) && GetContext(out UnitEntityData? unit)) {
            OnGui(bp!, true, unit!);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveUnitFactBA_RemoveText", "Remove")]
    private static partial string RemoveText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveUnitFactBA_Name", "Remove Fact")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveUnitFactBA_Description", "Removes the specified BlueprintUnitFact from the chosen unit.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveUnitFactBA_UnitDoesNotHaveThisFactText", "Unit does not have this Fact")]
    private static partial string UnitDoesNotHaveThisFactText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveUnitFactBA_RemoveSpellText", "Remove Spell")]
    private static partial string RemoveSpellText { get; }
}
