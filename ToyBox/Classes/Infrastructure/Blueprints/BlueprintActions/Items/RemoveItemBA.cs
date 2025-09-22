using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class RemoveItemBA : IBlueprintAction<BlueprintItem> {
    static RemoveItemBA() {
        BlueprintActions.RegisterAction((IBlueprintAction<SimpleBlueprint>)new RemoveItemBA());
    }
    private bool CanExecute(BlueprintItem blueprint, params object[] parameter) {
        return IsInGame();
    }
    private bool Execute(BlueprintItem blueprint, int count) {
        ((IBlueprintAction<SimpleBlueprint>)this).LogBPAction(blueprint, count);
        Game.Instance.Player.Inventory.Remove(blueprint, count);
        return true;
    }
    public bool? OnGui(BlueprintItem blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            int count = 1;
            if (parameter.Length > 0 && parameter[0] is int tmpCount) {
                count = tmpCount;
            }
            UI.Button(RemoveText + $" {count}", () => {
                result = Execute(blueprint, count);
            });
        }
        return result;
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveItemBA_Remove_x", "Remove")]
    private static partial string RemoveText { get; }
}
