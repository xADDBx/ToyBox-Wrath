namespace ToyBox.Infrastructure.Enums;
public enum CharacterListType {
    Party,
    PartyAndPets,
    AllCharacters,
    Active,
    Remote,
    CustomCompanions,
    Pets,
    Nearby,
    Friendly,
    Enemies,
    AllUnits
}
public static partial class CharacterListType_Localizer {
    public static string GetLocalized(this CharacterListType type) {
        return type switch {
            CharacterListType.Party => PartyText,
            CharacterListType.PartyAndPets => PartyAndPetsText,
            CharacterListType.AllCharacters => AllText,
            CharacterListType.Active => ActiveText,
            CharacterListType.Remote => RemoteText,
            CharacterListType.CustomCompanions => CustomCompanionText,
            CharacterListType.Pets => PetsText,
            CharacterListType.Nearby => NearbyText,
            CharacterListType.Friendly => FriendlyText,
            CharacterListType.Enemies => EnemiesText,
            CharacterListType.AllUnits => AllUnitsText,
            _ => "!!Error Unknown CharacterList!!",
        };
    }
    [LocalizedString("ToyBox_Infrastructure_Enums_CharacterListType_Localizer_PartyText", "Party")]
    private static partial string PartyText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_CharacterListType_Localizer_PetsText", "Party & Pets")]
    private static partial string PartyAndPetsText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_CharacterListType_Localizer_AllText", "All Characters")]
    private static partial string AllText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_CharacterListType_Localizer_ActiveText", "Active")]
    private static partial string ActiveText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_CharacterListType_Localizer_RemoteText", "Remote")]
    private static partial string RemoteText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_CharacterListType_Localizer_CustomCompanionText", "Custom Companions")]
    private static partial string CustomCompanionText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_CharacterListType_Localizer_PetsText", "Pets")]
    private static partial string PetsText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_CharacterListType_Localizer_NearbyText", "Nearby")]
    private static partial string NearbyText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_CharacterListType_Localizer_FriendlyText", "Friendly")]
    private static partial string FriendlyText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_CharacterListType_Localizer_EnemiesText", "Enemies")]
    private static partial string EnemiesText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_CharacterListType_Localizer_AllUnitsText", "All Units")]
    private static partial string AllUnitsText { get; }
}
