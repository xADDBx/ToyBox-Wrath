using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using ToyBox.Infrastructure.Utilities;
using UnityEngine;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class SpawnUnitBA : BlueprintActionFeature, IBlueprintAction<BlueprintUnit> {
    private bool CanExecute(BlueprintUnit blueprint, params object[] parameter) {
        return IsInGame();
    }
    private bool Execute(BlueprintUnit blueprint, int count) {
        LogExecution(blueprint, count);
        UnitEntityData? spawned = null;
        for (var i = 0; i < count; i++) {
            Vector3 spawnPosition = Game.Instance.ClickEventsController.WorldPosition;
            var offset = 5f * UnityEngine.Random.insideUnitSphere;
            spawnPosition = new(spawnPosition.x + offset.x, spawnPosition.y, spawnPosition.z + offset.z);
            spawned = Game.Instance.EntityCreator.SpawnUnit(blueprint, spawnPosition, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
        }
        return spawned != null;
    }
    public bool? OnGui(BlueprintUnit blueprint, bool isFeatureSearch = false, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            int count = 1;
            if (parameter.Length > 0 && parameter[0] is int tmpCount) {
                count = tmpCount;
            }
            UI.Button(StyleActionString(SpawnText + $" {count}", isFeatureSearch), () => {
                result = Execute(blueprint, count);
            });
        } else if (isFeatureSearch) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
        }
        return result;
    }
    public bool GetContext(out BlueprintUnit? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!, true);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_SpawnUnitBA_Spawn_x", "Spawn")]
    private static partial string SpawnText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_SpawnUnitBA_Name", "Spawn Unit")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_SpawnUnitBA_Description", "Spawns the specified unit in the vicinity.")]
    public override partial string Description { get; }
}
