using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class RemoveUnitFactBA : IBlueprintAction<BlueprintUnitFact> {
    static RemoveUnitFactBA() {
        BlueprintActions.RegisterAction((IBlueprintAction<SimpleBlueprint>)new RemoveUnitFactBA());
    }
    private bool CanExecute(BlueprintUnitFact blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return unit.GetFact(blueprint) != null;
        }
        return false;
    }
    private bool Execute(BlueprintUnitFact blueprint, params object[] parameter) {
        ((IBlueprintAction<SimpleBlueprint>)this).LogBPAction(blueprint, parameter);
        if (blueprint is BlueprintFeature feature) {
            foreach (var selection in ((UnitEntityData)parameter[0]).Progression.Selections) {
                foreach (var byLevel in selection.Value.SelectionsByLevel.ToList()) {
                    if (byLevel.Value.Contains(blueprint)) {
                        selection.Value.RemoveSelection(byLevel.Key, feature);
                        if (byLevel.Value.Count == 0) {
                            selection.Value.RemoveLevel(byLevel.Key);
                        }
                    }
                    if (selection.Value.SelectionsByLevel.Count == 0) {
                        ((UnitEntityData)parameter[0]).RemoveFact(selection.Key);
                    }
                }
            }
        }
            ((UnitEntityData)parameter[0]).RemoveFact(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintUnitFact blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(RemoveText, () => {
                result = Execute(blueprint, parameter);
            });
        }
        return result;
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveUnitFactBA_RemoveText", "Remove")]
    private static partial string RemoveText { get; }
}
