using Kingmaker;
using Kingmaker.Blueprints.Items;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class AddItemBA : BlueprintActionFeature, IBlueprintAction<BlueprintItem> {
    private bool CanExecute(BlueprintItem blueprint, params object[] parameter) {
        return IsInGame();
    }
    private bool Execute(BlueprintItem blueprint, int count) {
        LogExecution(blueprint, count);
        Game.Instance.Player.Inventory.Add(blueprint, count);
        return true;
    }
    public bool? OnGui(BlueprintItem blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            int count = 1;
            if (parameter.Length > 0 && parameter[0] is int tmpCount) {
                count = tmpCount;
            }
            UI.Button(AddText + $" {count}", () => {
                result = Execute(blueprint, count);
            });
        }
        return result;
    }

    public bool GetContext(out BlueprintItem? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddItemBA_Add_x", "Add")]
    private static partial string AddText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddItemBA_Name", "Add Item")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddItemBA_Description", "Adds the specified BlueprintItem to your inventory.")]
    public override partial string Description { get; }
}
