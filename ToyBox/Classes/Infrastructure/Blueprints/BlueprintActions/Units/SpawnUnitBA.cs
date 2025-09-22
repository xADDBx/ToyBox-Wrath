using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class SpawnUnitBA : IBlueprintAction<BlueprintUnit> {
    static SpawnUnitBA() {
        BlueprintActions.RegisterAction((IBlueprintAction<SimpleBlueprint>)new SpawnUnitBA());
    }
    private bool CanExecute(BlueprintUnit blueprint, params object[] parameter) {
        return IsInGame();
    }
    private bool Execute(BlueprintUnit blueprint, int count) {
        ((IBlueprintAction<SimpleBlueprint>)this).LogBPAction(blueprint, count);
        UnitEntityData? spawned = null;
        for (var i = 0; i < count; i++) {
            Vector3 spawnPosition = Game.Instance.ClickEventsController.WorldPosition;
            var offset = 5f * UnityEngine.Random.insideUnitSphere;
            spawnPosition = new(spawnPosition.x + offset.x, spawnPosition.y, spawnPosition.z + offset.z);
            spawned = Game.Instance.EntityCreator.SpawnUnit(blueprint, spawnPosition, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
        }
        return spawned != null;
    }
    public bool? OnGui(BlueprintUnit blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            int count = 1;
            if (parameter.Length > 0 && parameter[0] is int tmpCount) {
                count = tmpCount;
            }
            UI.Button(SpawnText + $" {count}", () => {
                result = Execute(blueprint, count);
            });
        }
        return result;
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_SpawnUnitBA_Spawn_x", "Spawn")]
    private static partial string SpawnText { get; }
}
