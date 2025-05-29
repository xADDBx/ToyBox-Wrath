namespace ToyBox.Infrastructure.Enums;
public enum UnitSelectType {
    Off,
    You,
    Party,
    Friendly,
    Enemies,
    Everyone,
}
public static partial class UnitSelectType_Localizer {
    public static string GetLocalized(this UnitSelectType type) {
        return type switch {
            UnitSelectType.Off => OffText,
            UnitSelectType.You => MainCharacterText,
            UnitSelectType.Party => PartyText,
            UnitSelectType.Friendly => FriendlyText,
            UnitSelectType.Enemies => EnemiesText,
            UnitSelectType.Everyone => EveryoneText,
            _ => "!!Error Unknown UnitSelectType!!",
        };
    }

    [LocalizedString("ToyBox_Infrastructure_Enums_UnitSelectType_Localizer_EveryoneText", "Everyone")]
    private static partial string EveryoneText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_UnitSelectType_Localizer_EnemiesText", "Enemies")]
    private static partial string EnemiesText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_UnitSelectType_Localizer_FriendlyText", "Friendly")]
    private static partial string FriendlyText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_UnitSelectType_Localizer_PartyText", "Party")]
    private static partial string PartyText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_UnitSelectType_Localizer_MainCharacterText", "Main Character")]
    private static partial string MainCharacterText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_UnitSelectType_Localizer_OffText", "Off")]
    private static partial string OffText { get; }
}
