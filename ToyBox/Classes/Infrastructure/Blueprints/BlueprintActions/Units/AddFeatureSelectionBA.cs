using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class AddFeatureSelectionBA : BlueprintActionFeature, IBlueprintAction<BlueprintFeatureSelection>, INeedContextFeature<BlueprintFeatureSelection, IFeatureSelectionItem>, INeedContextFeature<UnitEntityData> {
    private bool CanExecute(BlueprintFeatureSelection blueprint, params object[] parameter) {
        if (parameter.Length > 1 && parameter[0] is UnitEntityData unit && parameter[1] is IFeatureSelectionItem item) {
            if (item.Feature is BlueprintParametrizedFeature parametrized && item.Param == null) {
                return false;
            }
            return !unit.GetFacts<Kingmaker.UnitLogic.Feature>(item.Feature).Any(f => f.Param == item.Param);
        }
        return false;
    }
    private bool Execute(BlueprintFeatureSelection blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        var unit = (UnitEntityData)parameter[0];
        IFeatureSelectionItem item = (IFeatureSelectionItem)parameter[1];
        unit.Progression.AddSelection(blueprint, new(), 1, item.Feature);
        return unit.AddFact(item.Feature, null, item.Param) != null;
    }
    public bool? OnGui(BlueprintFeatureSelection blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(StyleActionString(AddText, isFeatureSearch), () => {
                result = Execute(blueprint, parameter);
            });
            UI.Label(" ");
        } else if (isFeatureSearch) {
            if (parameter[1] is IFeatureSelectionItem item && item.Feature is BlueprintParametrizedFeature && item.Param == null) {
                UI.Label(YouNeedToPickAParameterForTheFea.Red().Bold());
            } else {
                UI.Label(UnitAlreadyHasThisFactText.Red().Bold());
            }
        }

        return result;
    }
    public bool GetContext(out BlueprintFeatureSelection? context) => ContextProvider.Blueprint(out context);
    public bool GetContext(out UnitEntityData? context) => ContextProvider.UnitEntityData(out context);
    public bool GetContext(BlueprintFeatureSelection? data, out IFeatureSelectionItem? context) => ContextProvider.FeatureSelectionItemProvider(data, out context);
    public override void OnGui() {
        if (GetContext(out BlueprintFeatureSelection? bp) && GetContext(out UnitEntityData? unit) && GetContext(bp!, out IFeatureSelectionItem? item)) {
            OnGui(bp!, true, unit!, item!);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddFeatureSelectionBA_AddText", "Add")]
    private static partial string AddText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddFeatureSelectionBA_Name", "Add Feature Selection")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddFeatureSelectionBA_Description", "Adds the specified feature under the BlueprintFeatureSelection to the chosen unit.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddFeatureSelectionBA_UnitAlreadyHasThisFactText", "Unit already has the feature")]
    private static partial string UnitAlreadyHasThisFactText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddFeatureSelectionBA_YouNeedToPickAParameterForTheFea", "You need to pick a parameter for the feature list item!")]
    private static partial string YouNeedToPickAParameterForTheFea { get; }
}
