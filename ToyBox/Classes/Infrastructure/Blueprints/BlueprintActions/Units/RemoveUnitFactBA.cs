using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
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
    private bool Execute(BlueprintUnitFact blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
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
    public bool? OnGui(BlueprintUnitFact blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            var text = RemoveText;
            if (isFeatureSearch) {
                text = text.Cyan().Bold().SizePercent(115);
            }
            UI.Button(text, () => {
                result = Execute(blueprint, parameter);
            });
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
}
