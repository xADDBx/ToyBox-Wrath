using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class ChangeSpellbookLevelBA : BlueprintActionFeature, IBlueprintAction<BlueprintSpellbook>, INeedContextFeature<UnitEntityData> {
    private bool CanExecute(BlueprintSpellbook blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return !unit.Descriptor.m_Spellbooks.ContainsKey(blueprint);
        }
        return false;
    }
    private bool Execute(BlueprintSpellbook blueprint, Spellbook spellbook, params object[] parameter) {
        LogExecution(blueprint, parameter);
        if (spellbook.IsMythic) {
            spellbook.AddMythicLevel();
        } else {
            spellbook.AddBaseLevel();
        }
        return true;
    }
    public bool? OnGui(BlueprintSpellbook blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            var spellbook = ((UnitEntityData)parameter[0])!.Descriptor.DemandSpellbook(blueprint);
            /* We don't allow decreasing Spellbook Level as there is no corresponding event (aka unlearning spells)
            if (spellbook.CasterLevel > 0) {
                UI.Button("<", () => {
                    if (spellbook.IsMythic && !spellbook.IsStandaloneMythic) {
                        spellbook.m_MythicLevelInternal -= 1;
                    } else {
                        spellbook.m_BaseLevelInternal -= 1;
                    }
                });
            }
            */
            UI.Label($"{spellbook.CasterLevel} ");
            if (spellbook.CasterLevel < spellbook.MaxCasterLevel) {
                UI.Button(IncreaseCLText, () => {
                    result = Execute(blueprint, spellbook, parameter);
                });
            }
        }
        return result;
    }
    public bool GetContext(out BlueprintSpellbook? context) => ContextProvider.Blueprint(out context);
    public bool GetContext(out UnitEntityData? context) => ContextProvider.UnitEntityData(out context);
    public override void OnGui() {
        if (GetContext(out BlueprintSpellbook? bp) && GetContext(out UnitEntityData? unit)) {
            OnGui(bp!, unit!);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeSpellbookLevelBA__Plus1CasterLevelText", "+1 CL")]
    private static partial string IncreaseCLText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeSpellbookLevelBA_Name", "Increase spellbook level")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeSpellbookLevelBA_Description", "Increases the level of the specified BlueprintSpellbook on the chosen unit.")]
    public override partial string Description { get; }
}
