using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class AddSpellbookBA : BlueprintActionFeature, IBlueprintAction<BlueprintSpellbook>, INeedContextFeature<UnitEntityData> {
    public bool CanExecute(BlueprintSpellbook blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return !unit.Descriptor.m_Spellbooks.ContainsKey(blueprint);
        }
        return false;
    }
    private bool Execute(BlueprintSpellbook blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        return ((UnitEntityData)parameter[0])!.Descriptor.DemandSpellbook(blueprint) != null;
    }
    public bool? OnGui(BlueprintSpellbook blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(StyleActionString(AddText, isFeatureSearch), () => {
                result = Execute(blueprint, parameter);
            });
        } else if (isFeatureSearch) {
            UI.Label(UnitAlreadyHasThisSpellbookText.Red().Bold());
        }
        return result;
    }
    public bool GetContext(out BlueprintSpellbook? context) => ContextProvider.Blueprint(out context);
    public bool GetContext(out UnitEntityData? context) => ContextProvider.UnitEntityData(out context);
    public override void OnGui() {
        if (GetContext(out BlueprintSpellbook? bp) && GetContext(out UnitEntityData? unit)) {
            OnGui(bp!, true, unit!);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddSpellbookBA_AddText", "Add")]
    private static partial string AddText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddSpellbookBA_Name", "Add Spellbook")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddSpellbookBA_Description", "Adds the specified BlueprintSpellbook to the chosen unit.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddSpellbookBA_UnitAlreadyHasThisSpellbookText", "Unit already has this Spellbook")]
    private static partial string UnitAlreadyHasThisSpellbookText { get; }
}
