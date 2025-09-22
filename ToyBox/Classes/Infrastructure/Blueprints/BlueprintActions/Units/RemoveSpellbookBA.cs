using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class RemoveSpellbookBA : IBlueprintAction<BlueprintSpellbook> {
    static RemoveSpellbookBA() {
        BlueprintActions.RegisterAction((IBlueprintAction<SimpleBlueprint>)new RemoveSpellbookBA());
    }
    private bool CanExecute(BlueprintSpellbook blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return unit.Descriptor.m_Spellbooks.ContainsKey(blueprint);
        }
        return false;
    }
    private bool Execute(BlueprintSpellbook blueprint, params object[] parameter) {
        if (CanExecute(blueprint, parameter)) {
            ((UnitEntityData)parameter[0])!.Descriptor.DeleteSpellbook(blueprint);
            return true;
        }
        return false;
    }
    public void OnGui(BlueprintSpellbook blueprint, params object[] parameter) {
        if (CanExecute(blueprint, parameter)) {
            UI.Button(RemoveText, () => {
                Execute(blueprint, parameter);
            });
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveSpellbookBA_RemoveText", "Remove")]
    private static partial string RemoveText { get; }
}
