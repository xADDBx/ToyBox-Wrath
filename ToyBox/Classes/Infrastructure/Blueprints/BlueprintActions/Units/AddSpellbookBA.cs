using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class AddSpellbookBA : IBlueprintAction<BlueprintSpellbook> {
    static AddSpellbookBA() {
        BlueprintActions.RegisterAction((IBlueprintAction<SimpleBlueprint>)new AddSpellbookBA());
    }
    private bool CanExecute(BlueprintSpellbook blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return !unit.Descriptor.m_Spellbooks.ContainsKey(blueprint);
        }
        return false;
    }
    private bool Execute(BlueprintSpellbook blueprint, params object[] parameter) {
        if (CanExecute(blueprint, parameter)) {
            return ((UnitEntityData)parameter[0])!.Descriptor.DemandSpellbook(blueprint) != null;
        }
        return false;
    }
    public void OnGui(BlueprintSpellbook blueprint, params object[] parameter) {
        if (CanExecute(blueprint, parameter)) {
            UI.Button(AddText, () => {
                Execute(blueprint, parameter);
            });
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddSpellbookBA_AddText", "Add")]
    private static partial string AddText { get; }
}
