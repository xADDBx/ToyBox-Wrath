namespace ToyBox.Infrastructure.Enums;
public enum PartyTabSectionType {
    None,
    Classes,
    Stats,
    Features,
    Buffs,
    Abilities,
    Spells,
    Inspect
}
public static partial class PartyTabSectionType_Localizer {
    public static string GetLocalized(this PartyTabSectionType type) {
        return type switch {
            PartyTabSectionType.None => SharedStrings.NoneText,
            PartyTabSectionType.Classes => ClassesText,
            PartyTabSectionType.Stats => StatsText,
            PartyTabSectionType.Features => FeaturesText,
            PartyTabSectionType.Buffs => BuffsText,
            PartyTabSectionType.Abilities => AbilitiesText,
            PartyTabSectionType.Spells => SpellsText,
            PartyTabSectionType.Inspect => InspectText,
            _ => "!!Error Unknown PartyTabSectionType!!",
        };
    }
    [LocalizedString("ToyBox_Infrastructure_Enums_PartyTabSectionType_Localizer_ClassesText", "Classes")]
    private static partial string ClassesText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_PartyTabSectionType_Localizer_StatsText", "Stats")]
    private static partial string StatsText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_PartyTabSectionType_Localizer_FeaturesText", "Features")]
    private static partial string FeaturesText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_PartyTabSectionType_Localizer_BuffsText", "Buffs")]
    private static partial string BuffsText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_PartyTabSectionType_Localizer_AbilitiesText", "Abilities")]
    private static partial string AbilitiesText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_PartyTabSectionType_Localizer_SpellsText", "Spells")]
    private static partial string SpellsText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_PartyTabSectionType_Localizer_InspectText", "Inspect")]
    private static partial string InspectText { get; }
}
