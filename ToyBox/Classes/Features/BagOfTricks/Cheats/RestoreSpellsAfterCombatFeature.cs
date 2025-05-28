using Kingmaker;
using Kingmaker.PubSubSystem;

namespace ToyBox.Features.BagOfTricks.Cheats;

public partial class RestoreSpellsAfterCombatFeature : ToggledFeature, IPartyCombatHandler {
    public override ref bool IsEnabled => ref Settings.RestoreSpellsAfterCombat;

    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_RestoreSpellsAfterCombatFeature_RestoreSpellsAfterCombatText", "Restore Spells After Combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_RestoreSpellsAfterCombatFeature_RestoreSpellChargesAfterCombatTe", "Restore spell charges after combat")]
    public override partial string Description { get; }
    public override void Initialize() => new Action(() => EventBus.Subscribe(this)).ScheduleForMainThread();
    public override void Destroy() => new Action(() => EventBus.Unsubscribe(this)).ScheduleForMainThread();
    public void HandlePartyCombatStateChanged(bool inCombat) {
        if (!inCombat) {
            foreach (var unit in Game.Instance.Player.Party) {
                foreach (var spellbook in unit.Descriptor.Spellbooks) {
                    spellbook.Rest();
                }
            }
        }
    }
}
