using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;

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
        ((IBlueprintAction<SimpleBlueprint>)this).LogBPAction(blueprint, parameter);
        ((UnitEntityData)parameter[0])!.Descriptor.DeleteSpellbook(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintSpellbook blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(RemoveText, () => {
                result = Execute(blueprint, parameter);
            });
        }
        return result;
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveSpellbookBA_RemoveText", "Remove")]
    private static partial string RemoveText { get; }
}
