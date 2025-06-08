using Kingmaker.EntitySystem.Entities;
using Kingmaker;
using Kingmaker.UnitLogic;
using Kingmaker.Designers;
using UnityEngine;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure;
public static partial class CharacterPicker {
    private static readonly int m_CacheDuration = 1;
    private static Dictionary<CharacterListType, TimedCache<List<UnitEntityData>>> m_Lists = new() {
        [CharacterListType.Party] = new(() => Game.Instance.Player.Party ?? [], m_CacheDuration),
        [CharacterListType.PartyAndPets] = new(() => Game.Instance.Player.PartyAndPets ?? [], m_CacheDuration),
        [CharacterListType.AllCharacters] = new(() => Game.Instance.Player.AllCharacters ?? [], m_CacheDuration),
        [CharacterListType.Active] = new(() => Game.Instance.Player.ActiveCompanions ?? [], m_CacheDuration),
        [CharacterListType.Remote] = new(() => Game.Instance.Player.RemoteCompanions?.ToList() ?? [], m_CacheDuration),
        [CharacterListType.CustomCompanions] = new(() => Game.Instance.Player.AllCharacters.Where(u => u.IsCustomCompanion())?.ToList() ?? [], m_CacheDuration),
        [CharacterListType.Pets] = new(() => Game.Instance.Player.AllCharacters.Where(u => u.IsPet)?.ToList() ?? [], m_CacheDuration),
        [CharacterListType.Nearby] = new(() => {
            var player = GameHelper.GetPlayerCharacter();
            return GameHelper.GetTargetsAround(player.Position, NearbyRange, false, false)?.ToList() ?? [];
        }, m_CacheDuration),
        [CharacterListType.Friendly] = new(() => {
            var player = GameHelper.GetPlayerCharacter();
            return Game.Instance.State.Units.Where(u => u != null && !u.IsEnemy(player))?.ToList() ?? [];
        }, m_CacheDuration),
        [CharacterListType.Enemies] = new(() => {
            var player = GameHelper.GetPlayerCharacter();
            return Game.Instance.State.Units.Where(u => u != null && u.IsEnemy(player))?.ToList() ?? [];
        }, m_CacheDuration),
        [CharacterListType.AllUnits] = new(() => Game.Instance.State.Units?.ToList() ?? [], m_CacheDuration)
    };
    private static CharacterListType m_CurrentList;
    private static WeakReference<UnitEntityData>? m_CurrentUnit;
    public static UnitEntityData? CurrentUnit {
        get {
            if (m_CurrentUnit is not null && m_CurrentUnit.TryGetTarget(out var unit) && !unit.IsDisposed && !unit.IsDisposingNow) {
                return unit;
            } else {
                return null;
            }
        }
    }
    public static List<UnitEntityData> CurrentUnits {
        get {
            return m_Lists[m_CurrentList];
        }
    }
#warning Expose to user
    public static float NearbyRange = 25;
    public static List<UnitEntityData> OnFilterPickerGUI(int? xcols = null, params GUILayoutOption[] options) {
        if (!IsInGame()) {
            UI.UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red());
            return [];
        }
        if (UI.UI.SelectionGrid(ref m_CurrentList, xcols ?? Math.Min(11, m_Lists.Count), type => type.GetLocalized(), options)) {
            m_CurrentUnit = null;
        }
        return CurrentUnits;
    }
    public static UnitEntityData? OnCharacterPickerGUI(int? xcols = null, params GUILayoutOption[] options) {
        if (!IsInGame()) {
            UI.UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red());
            return null;
        }
        var charactersList = CurrentUnits;
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
