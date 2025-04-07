using Kingmaker;
using Kingmaker.PubSubSystem;

namespace ToyBox.Features.BagOfTricks;

public partial class RestoreItemsAfterCombatFeature : ToggledFeature, IPartyCombatHandler {
    public override ref bool IsEnabled => ref Settings.RestoreItemsAfterCombat;

    [LocalizedString("ToyBox_Features_BagOfTricks_RestoreItemsAfterCombatFeature_RestoreItemsAfterCombatText", "Restore Items After Combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RestoreItemsAfterCombatFeature_RestoreItemChargesAfterCombatTex", "Restore item charges after combat")]
    public override partial string Description { get; }
    public override void Initialize() => new Action(() => EventBus.Subscribe(this)).ScheduleForMainThread();
    public override void Destroy() => new Action(() => EventBus.Unsubscribe(this)).ScheduleForMainThread();
    public void HandlePartyCombatStateChanged(bool inCombat) {
        if (!inCombat) {
            foreach (var unit in Game.Instance.Player.Party) {
                foreach (var item in unit.Inventory.Items)
                    item.RestoreCharges();
            }
        }
    }
}
