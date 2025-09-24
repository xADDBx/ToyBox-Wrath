using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class AddUnitFactBA : BlueprintActionFeature, IBlueprintAction<BlueprintUnitFact>, INeedContextFeature<UnitEntityData> {
    private bool CanExecute(BlueprintUnitFact blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return unit.GetFact(blueprint) == null;
        }
        return false;
    }
    private bool Execute(BlueprintUnitFact blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        return ((UnitEntityData)parameter[0]).AddFact(blueprint) != null;
    }
    public bool? OnGui(BlueprintUnitFact blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(AddText, () => {
                result = Execute(blueprint, parameter);
            });
        }
        return result;
    }
    public bool GetContext(out BlueprintUnitFact? context) => ContextProvider.Blueprint(out context);
    public bool GetContext(out UnitEntityData? context) => ContextProvider.UnitEntityData(out context);
    public override void OnGui() {
        if (GetContext(out BlueprintUnitFact? bp) && GetContext(out UnitEntityData? unit)) {
            OnGui(bp!, unit!);
        }
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddUnitFactBA_AddText", "Add")]
    private static partial string AddText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddUnitFactBA_Name", "Add Fact")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddUnitFactBA_Description", "Adds the specified BlueprintUnitFact to the chosen unit.")]
    public override partial string Description { get; }
}
