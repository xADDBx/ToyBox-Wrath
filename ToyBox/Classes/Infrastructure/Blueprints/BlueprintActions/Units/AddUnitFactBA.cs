using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class AddUnitFactBA : BlueprintActionFeature, IBlueprintAction<BlueprintUnitFact>, INeedContextFeature<UnitEntityData> {
    public bool CanExecute(BlueprintUnitFact blueprint, params object[] parameter) {
        if (parameter.Length > 0) {
            if (parameter[0] is bool isSpell) {
                return CanExecute(blueprint, isSpell, out _, parameter.Skip(1));
            } else {
                return CanExecute(blueprint, false, out _, parameter);
            }
        } else {
            return false;
        }
    }
    internal bool CanExecute(BlueprintUnitFact blueprint, bool isSpell, out (Spellbook, int)? sb, params object[] parameter) {
        sb = null;
        if (blueprint is IFeatureSelection) {
            return false;
        }
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            if (isSpell) {
                foreach (var spellbook in unit.Spellbooks) {
                    var ability = blueprint as BlueprintAbility;
                    if (spellbook.IsKnown(ability)) {
                        return false;
                    }
                    var spellbookBP = spellbook.Blueprint;
                    var maxLevel = spellbookBP.MaxSpellLevel;
                    for (var level = 0; level <= maxLevel; level++) {
                        var learnable = spellbookBP.SpellList.GetSpells(level);
                        if (learnable.Contains(ability)) {
                            sb = (spellbook, level);
                            return true;
                        }
                    }
                }
            } else {
                return unit.GetFact(blueprint) == null;
            }
        }
        return false;
    }
    private bool CanExecuteAtWill(BlueprintUnitFact blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return unit.GetFact(blueprint) == null;
        }
        return false;
    }
    internal bool Execute(BlueprintUnitFact blueprint, bool isSpell, (Spellbook, int)? maybeSpellbook, params object[] parameter) {
        LogExecution(blueprint, isSpell, maybeSpellbook, parameter);
        if (isSpell && maybeSpellbook.HasValue) {
            return maybeSpellbook.Value.Item1.AddKnown(maybeSpellbook.Value.Item2, blueprint as BlueprintAbility) != null;
        } else {
            return ((UnitEntityData)parameter[0]).AddFact(blueprint) != null;
        }
    }
    private bool ExecuteAtWill(BlueprintUnitFact blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        return ((UnitEntityData)parameter[0]).AddFact(blueprint) != null;
    }
    public bool? OnGui(BlueprintUnitFact blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        var isSpell = blueprint is BlueprintAbility ability && ability.IsSpell;
        if (CanExecute(blueprint, isSpell, out var maybeSpellbookToAdd, parameter)) {
            UI.Button(StyleActionString(AddText, isFeatureSearch), () => {
                result = Execute(blueprint, isSpell, maybeSpellbookToAdd, parameter);
            });
            UI.Label(" ");
        } else if (isFeatureSearch) {
            if (isSpell) {
                UI.Label(SpellAlreadyIsKnownInFirstSpellb.Red().Bold());
            } else {
                if (blueprint is IFeatureSelection) {
                    UI.Label(CantAddFeatureSelectionsViaTheA.Red().Bold());
                } else {
                    UI.Label(UnitAlreadyHasThisFactText.Red().Bold());
                }
            }
        }

        if (isSpell) {
            if (CanExecuteAtWill(blueprint, parameter)) {
                UI.Button(StyleActionString(AtWillText, isFeatureSearch), () => {
                    result = ExecuteAtWill(blueprint, parameter);
                });
            } else if (isFeatureSearch) {
                UI.Label(UnitAlreadyHasThisFactText.Red().Bold());
            }
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
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddUnitFactBA_AddText", "Add")]
    private static partial string AddText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddUnitFactBA_Name", "Add Fact")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddUnitFactBA_Description", "Adds the specified BlueprintUnitFact to the chosen unit.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddUnitFactBA_UnitAlreadyHasThisFactText", "Unit already has this Fact")]
    private static partial string UnitAlreadyHasThisFactText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddUnitFactBA_SpellAlreadyIsKnownInFirstSpellb", "Spell already is known in first Spellbook")]
    private static partial string SpellAlreadyIsKnownInFirstSpellb { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddUnitFactBA_AtWillText", "At Will")]
    private static partial string AtWillText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddUnitFactBA_Can_tAddFeatureSelectionsViaTheA", "Can't add Feature Selections via the Add Unit Fact action. Use the Parametrized or Feature Selection variants instead!")]
    private static partial string CantAddFeatureSelectionsViaTheA { get; }
}
