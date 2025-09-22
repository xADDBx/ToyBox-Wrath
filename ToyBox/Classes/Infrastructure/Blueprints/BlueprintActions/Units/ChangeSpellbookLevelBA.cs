using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class ChangeSpellbookLevelBA : IBlueprintAction<BlueprintSpellbook> {
    static ChangeSpellbookLevelBA() {
        BlueprintActions.RegisterAction((IBlueprintAction<SimpleBlueprint>)new ChangeSpellbookLevelBA());
    }
    private bool CanExecute(BlueprintSpellbook blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return !unit.Descriptor.m_Spellbooks.ContainsKey(blueprint);
        }
        return false;
    }
    private bool Execute(BlueprintSpellbook blueprint, Spellbook spellbook, params object[] parameter) {
        ((IBlueprintAction<SimpleBlueprint>)this).LogBPAction(blueprint, parameter);
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

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeSpellbookLevelBA__Plus1CasterLevelText", "+1 CL")]
    private static partial string IncreaseCLText { get; }
}
