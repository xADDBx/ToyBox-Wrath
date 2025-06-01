using Kingmaker.EntitySystem.Entities;
using Kingmaker;
using Kingmaker.UnitLogic;
using Kingmaker.Designers;
using UnityEngine;

namespace ToyBox.Infrastructure;
public static partial class CharacterPicker {
    private static Dictionary<CharacterListType, CharacterList> m_Lists = new() {
        [CharacterListType.Party] = new(() => Game.Instance.Player.Party),
        [CharacterListType.PartyAndPets] = new(() => Game.Instance.Player.PartyAndPets),
        [CharacterListType.AllCharacters] = new(() => Game.Instance.Player.AllCharacters),
        [CharacterListType.Active] = new(() => Game.Instance.Player.ActiveCompanions),
        [CharacterListType.Remote] = new(() => Game.Instance.Player.RemoteCompanions),
        [CharacterListType.CustomCompanions] = new(() => Game.Instance.Player.AllCharacters.Where(u => u.IsCustomCompanion())),
        [CharacterListType.Pets] = new(() => Game.Instance.Player.AllCharacters.Where(u => u.IsPet)),
        [CharacterListType.Nearby] = new(() => {
            var player = GameHelper.GetPlayerCharacter();
            return GameHelper.GetTargetsAround(player.Position, NearbyRange, false, false);
        }),
        [CharacterListType.Friendly] = new(() => {
            var player = GameHelper.GetPlayerCharacter();
            return Game.Instance.State.Units.Where(u => u != null && !u.IsEnemy(player));
        }),
        [CharacterListType.Enemies] = new(() => {
            var player = GameHelper.GetPlayerCharacter();
            return Game.Instance.State.Units.Where(u => u != null && u.IsEnemy(player));
        }),
        [CharacterListType.AllUnits] = new(() => Game.Instance.State.Units)
    };
    private static CharacterListType m_CurrentList;
    private static WeakReference<UnitEntityData>? m_CurrentUnit;
    public class CharacterList {
        private readonly Func<IEnumerable<UnitEntityData>?> m_GetUnitsFunc;
        public CharacterList(Func<IEnumerable<UnitEntityData>> getUnits) {
#warning implement something like 1 second caching to prevent issues with expensive functions
            m_GetUnitsFunc = getUnits;
        }
        public IEnumerable<UnitEntityData> Units => m_GetUnitsFunc() ?? [];
        public List<UnitEntityData> UnitsList => m_GetUnitsFunc()?.ToList() ?? [];
    }
    public static UnitEntityData? CurrentUnit {
        get {
            if (m_CurrentUnit is not null && m_CurrentUnit.TryGetTarget(out var unit) && !unit.IsDisposed && !unit.IsDisposingNow) {
                return unit;
            } else {
                return null;
            }
        }
    }
    public static CharacterList CurrentList {
        get {
            return m_Lists[m_CurrentList];
        }
    }
#warning Expose to user
    public static float NearbyRange = 25;
    public static List<UnitEntityData> GetUnitsFromCurrentList() {
        return CurrentList.UnitsList;
    }
    public static CharacterList OnFilterPickerGUI(int? xcols = null, params GUILayoutOption[] options) {
        if (!IsInGame()) {
            UI.UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red());
            return CurrentList;
        }
        if (UI.UI.SelectionGrid(ref m_CurrentList, xcols ?? Math.Min(11, m_Lists.Count), type => type.GetLocalized(), options)) {
            m_CurrentUnit = null;
        }
        return CurrentList;
    }
    public static UnitEntityData? OnCharacterPickerGUI(int? xcols = null, params GUILayoutOption[] options) {
        if (!IsInGame()) {
            UI.UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red());
            return null;
        }
        var charactersList = CurrentList.UnitsList;
        if (charactersList.Count == 0) {
            UI.UI.Label(ThereAreNoCharactersInThisList.Orange(), options);
        } else {
            var tmp = CurrentUnit;
            if (UI.UI.SelectionGrid(ref tmp, charactersList, xcols ?? Math.Min(8, (charactersList.Count + 1)), unit => ToyBoxUnitHelper.GetUnitName(unit), options)) {
                if (tmp != null) {
                    m_CurrentUnit = new(tmp);
                } else {
                    m_CurrentUnit = null;
                }
            }
        }
        return CurrentUnit;
    }

    [LocalizedString("ToyBox_Infrastructure_CharacterPicker_ThereAreNoCharactersInThisList", "There are no characters in this list!")]
    private static partial string ThereAreNoCharactersInThisList { get; }
}
