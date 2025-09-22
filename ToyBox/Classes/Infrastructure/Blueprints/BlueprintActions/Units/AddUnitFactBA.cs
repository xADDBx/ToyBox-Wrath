using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class AddUnitFactBA : IBlueprintAction<BlueprintUnitFact> {
    static AddUnitFactBA() {
        BlueprintActions.RegisterAction((IBlueprintAction<SimpleBlueprint>)new AddUnitFactBA());
    }
    private bool CanExecute(BlueprintUnitFact blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return unit.GetFact(blueprint) == null;
        }
        return false;
    }
    private bool Execute(BlueprintUnitFact blueprint, params object[] parameter) {
        if (CanExecute(blueprint, parameter)) {
            return ((UnitEntityData)parameter[0]).AddFact(blueprint) != null;
        }
        return false;
    }
    public void OnGui(BlueprintUnitFact blueprint, params object[] parameter) {
        if (CanExecute(blueprint, parameter)) {
            UI.Button(AddText, () => {
                Execute(blueprint, parameter);
            });
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddUnitFactBA_AddText", "Add")]
    private static partial string AddText { get; }
}
