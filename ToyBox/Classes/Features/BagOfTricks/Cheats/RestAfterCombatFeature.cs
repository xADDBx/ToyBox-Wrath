using Kingmaker.Cheats;
using Kingmaker.PubSubSystem;

namespace ToyBox.Features.BagOfTricks.Cheats;

public partial class RestAfterCombatFeature : ToggledFeature, IPartyCombatHandler {
    public override ref bool IsEnabled => ref Settings.RestAfterCombat;

    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_RestAfterCombatFeature_RestPartyInstantlyAfterCombatTex", "Rest Party Instantly After Combat")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Cheats_RestAfterCombatFeature_RestAllPartyMembersInstantlyAfte", "Rest all party members instantly after combat")]
    public override partial string Description { get; }
    public override void Initialize() => new Action(() => EventBus.Subscribe(this)).ScheduleForMainThread();
    public override void Destroy() => new Action(() => EventBus.Unsubscribe(this)).ScheduleForMainThread();
    public void HandlePartyCombatStateChanged(bool inCombat) {
        if (!inCombat) {
            CheatsCombat.RestAll();
        }
    }
}
