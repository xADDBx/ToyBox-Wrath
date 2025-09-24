using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class RemoveSpellbookBA : BlueprintActionFeature, IBlueprintAction<BlueprintSpellbook>, INeedContextFeature<UnitEntityData> {
    private bool CanExecute(BlueprintSpellbook blueprint, params object[] parameter) {
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            return unit.Descriptor.m_Spellbooks.ContainsKey(blueprint);
        }
        return false;
    }
    private bool Execute(BlueprintSpellbook blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        ((UnitEntityData)parameter[0])!.Descriptor.DeleteSpellbook(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintSpellbook blueprint, bool isFeatureSearch, params object[] parameter) {
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
            UI.Label(UnitDoesNotHaveThisSpellbookText.Red().Bold());
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

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveSpellbookBA_RemoveText", "Remove")]
    private static partial string RemoveText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveSpellbookBA_Name", "Remove Spellbook")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveSpellbookBA_Description", "Remove the specified BlueprintSpellbook from the chosen unit.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveSpellbookBA_UnitDoesNotHaveThisSpellbookText", "Unit does not have this Spellbook")]
    private static partial string UnitDoesNotHaveThisSpellbookText { get; }
}
