using Kingmaker;
using Kingmaker.PubSubSystem;

namespace ToyBox.Features.BagOfTricks.Cheats;

public partial class RestoreAbilitiesAfterCombatFeature : ToggledFeature, IPartyCombatHandler {
    public override ref bool IsEnabled => ref Settings.RestoreAbilitiesAfterCombat;

    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_RestoreAbilitiesAfterCombatFeature_RestoreAbilitiesAfterCombatText", "Restore Abilities After Combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_RestoreAbilitiesAfterCombatFeature_RestoresAllChargesOnAbilitiesAft", "Restores all charges on abilities after combat")]
    public override partial string Description { get; }
    public override void Initialize() => new Action(() => EventBus.Subscribe(this)).ScheduleForMainThread();
    public override void Destroy() => new Action(() => EventBus.Unsubscribe(this)).ScheduleForMainThread();
    public void HandlePartyCombatStateChanged(bool inCombat) {
        if (!inCombat) {
            foreach (var unit in Game.Instance.Player.Party) {
                foreach (var resource in unit.Descriptor.Resources) {
                    unit.Descriptor.Resources.Restore(resource);
                }
                unit.Brain.RestoreAvailableActions();
            }
        }
    }
}
