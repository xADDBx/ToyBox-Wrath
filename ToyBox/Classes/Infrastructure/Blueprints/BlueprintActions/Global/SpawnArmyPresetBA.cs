using Kingmaker;
using Kingmaker.Armies.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class SpawnArmyPresetBA : BlueprintActionFeature, IBlueprintAction<BlueprintArmyPreset> {
    public bool CanExecute(BlueprintArmyPreset blueprint, params object[] parameter) {
        return IsInGame()
            && Game.Instance.Player.GlobalMap.CurrentPosition != null;
    }
    private bool Execute(BlueprintArmyPreset blueprint, bool friendly, params object[] parameter) {
        var faction = friendly ? Kingmaker.Armies.ArmyFaction.Crusaders : Kingmaker.Armies.ArmyFaction.Demons;
        var position = Game.Instance.Player.GlobalMap.CurrentPosition;
        LogExecution(blueprint, faction, position, parameter);
        Game.Instance.Player.GlobalMap.LastActivated.CreateArmy(faction, blueprint, position);
        return true;
    }
    public bool? OnGui(BlueprintArmyPreset blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(StyleActionString(SpawnFriendlyText, isFeatureSearch), () => {
                result = Execute(blueprint, true, parameter);
            });
            UI.Label(" ");
            UI.Button(StyleActionString(SpawnHostileText, isFeatureSearch), () => {
                result = Execute(blueprint, false, parameter);
            });
        } else if (isFeatureSearch) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
        }
        return result;
    }

    public bool GetContext(out BlueprintArmyPreset? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!, true);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_SpawnArmyPresetBA_TeleportText", "Spawn")]
    private static partial string TeleportText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_SpawnArmyPresetBA_Name", "Spawn an army")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_SpawnArmyPresetBA_Description", "Spawns the specified BlueprintArmyPreset as friend or foe.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_SpawnArmyPresetBA_SpawnFriendlyText", "Spawn Friendly")]
    private static partial string SpawnFriendlyText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_SpawnArmyPresetBA_SpawnHostileText", "Spawn Hostile")]
    private static partial string SpawnHostileText { get; }
}
