using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class AddParametrizedFeatureBA : BlueprintActionFeature, IBlueprintAction<BlueprintParametrizedFeature>, INeedContextFeature<BlueprintParametrizedFeature, IFeatureSelectionItem>, INeedContextFeature<UnitEntityData> {
    public bool CanExecute(BlueprintParametrizedFeature blueprint, params object[] parameter) {
        if (parameter.Length > 1 && parameter[0] is UnitEntityData unit && parameter[1] is IFeatureSelectionItem item) {
            return !unit.GetFacts<Kingmaker.UnitLogic.Feature>(item.Feature).Any(f => f.Param == item.Param);
        }
        return false;
    }
    private bool Execute(BlueprintParametrizedFeature blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        var item = (IFeatureSelectionItem)parameter[1];
        return ((UnitEntityData)parameter[0]).AddFact(item.Feature, null, item.Param) != null;
    }
    public bool? OnGui(BlueprintParametrizedFeature blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(StyleActionString(AddText, isFeatureSearch), () => {
                result = Execute(blueprint, parameter);
            });
            UI.Label(" ");
        } else if (isFeatureSearch) {
            UI.Label(UnitAlreadyHasThisFactText.Red().Bold());
        }

        return result;
    }
    public bool GetContext(out BlueprintParametrizedFeature? context) => ContextProvider.Blueprint(out context);
    public bool GetContext(out UnitEntityData? context) => ContextProvider.UnitEntityData(out context);
    public bool GetContext(BlueprintParametrizedFeature? data, out IFeatureSelectionItem? context) => ContextProvider.FeatureSelectionItemProvider(data, out context);
    public override void OnGui() {
        if (GetContext(out BlueprintParametrizedFeature? bp) && GetContext(out UnitEntityData? unit) && GetContext(bp!, out var item)) {
            OnGui(bp!, true, unit!, item!);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddParametrizedFeatureBA_AddText", "Add")]
    private static partial string AddText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddParametrizedFeatureBA_Name", "Add Parametrized Feature")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddParametrizedFeatureBA_Description", "Adds the BlueprintParametrizedFeature with the specified parameter to the chosen unit.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddParametrizedFeatureBA_UnitAlreadyHasThisFactText", "Unit already has the feature with this parameter")]
    private static partial string UnitAlreadyHasThisFactText { get; }
}
