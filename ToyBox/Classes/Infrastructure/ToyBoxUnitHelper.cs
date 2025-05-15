using Kingmaker;
using Kingmaker.EntitySystem.Entities;

namespace ToyBox.Infrastructure;
public static class ToyBoxUnitHelper {    private static Dictionary<UnitEntityData, bool> m_PartyOrPetCache = new();    private static bool m_IsInitialized = false;    internal static void Initialize() {        if (m_IsInitialized) return;        Main.HarmonyInstance.Patch(AccessTools.Method(typeof(Player), nameof(Player.InvalidateCharacterLists)), new(() => m_PartyOrPetCache.Clear()));        m_IsInitialized = true;    }
    public static bool IsPartyOrPet(UnitEntityData? unit) {        if (unit == null) return false;        if (m_PartyOrPetCache.TryGetValue(unit, out bool result)) return result;
        if (unit.OriginalBlueprint == null || Game.Instance?.Player?.AllCharacters is { Count: 0 }) {
            return false;
        }
        var isPartyOrPet = Game.Instance!.Player!.AllCharacters!.Any(x => x.OriginalBlueprint == unit.OriginalBlueprint
                             && (x.Master == null || x.Master.OriginalBlueprint == null                             || Game.Instance.Player.AllCharacters.Any(y => y.OriginalBlueprint == x.Master.OriginalBlueprint)));        m_PartyOrPetCache[unit] = isPartyOrPet;
        return isPartyOrPet;
    }
}
