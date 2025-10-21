using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class RemoveAbilityResourceBA : BlueprintActionFeature, IBlueprintAction<BlueprintAbilityResource>, INeedContextFeature<UnitEntityData> {
    public bool CanExecute(BlueprintAbilityResource blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return unit.Resources.ContainsResource(blueprint);
        }
        return false;
    }
    private bool Execute(BlueprintAbilityResource blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        ((UnitEntityData)parameter[0])!.Resources.Remove(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintAbilityResource blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(StyleActionString(RemoveText, isFeatureSearch), () => {
                result = Execute(blueprint, parameter);
            });
        } else if (isFeatureSearch) {
            UI.Label(UnitAlreadyHasThisAbilityResourc.Red().Bold());
        }
        return result;
    }
    public override void OnGui() {
        if (GetContext(out BlueprintAbilityResource? bp) && GetContext(out UnitEntityData? unit)) {
            OnGui(bp!, true, unit!);
        }
    }
    public bool GetContext(out UnitEntityData? context) => ContextProvider.UnitEntityData(out context);
    public bool GetContext(out BlueprintAbilityResource? context) => ContextProvider.Blueprint(out context);
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveAbilityResourceBA_Name", "Remove Ability Resource")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveAbilityResourceBA_Description", "Removes the specified BlueprintAbilityResource from the chosen unit.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveAbilityResourceBA_RemoveText", "Remove")]
    private static partial string RemoveText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveAbilityResourceBA_UnitAlreadyHasThisAbilityResourc", "Unit does not have this ability resource")]
    private static partial string UnitAlreadyHasThisAbilityResourc { get; }
}
