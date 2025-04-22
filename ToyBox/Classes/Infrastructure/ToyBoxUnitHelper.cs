using Kingmaker;
using Kingmaker.EntitySystem.Entities;

namespace ToyBox.Infrastructure;
public static class ToyBoxUnitHelper {
    public static bool IsPartyOrPet(UnitEntityData? unit) {
        if (unit == null || unit.OriginalBlueprint == null || Game.Instance?.Player?.AllCharacters is { Count: 0 }) {
            return false;
        }

        return Game.Instance!.Player!.AllCharacters!.Any(x => x.OriginalBlueprint == unit.OriginalBlueprint
                             && (x.Master == null || x.Master.OriginalBlueprint == null 
                             || Game.Instance.Player.AllCharacters.Any(y => y.OriginalBlueprint == x.Master.OriginalBlueprint)));
    }
}
