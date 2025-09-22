using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class AddItemBA : IBlueprintAction<BlueprintItem> {
    static AddItemBA() {
        BlueprintActions.RegisterAction((IBlueprintAction<SimpleBlueprint>)new AddItemBA());
    }
    private bool CanExecute(BlueprintItem blueprint, params object[] parameter) {
        return IsInGame();
    }
    private bool Execute(BlueprintItem blueprint, int count) {
        ((IBlueprintAction<SimpleBlueprint>)this).LogBPAction(blueprint, count);
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
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddItemBA_Add_x", "Add")]
    private static partial string AddText { get; }
}
