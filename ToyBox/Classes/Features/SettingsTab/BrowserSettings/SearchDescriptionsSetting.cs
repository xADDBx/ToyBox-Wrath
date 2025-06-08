namespace ToyBox.Features.SettingsFeatures.BrowserSettings;

public partial class SearchDescriptionsSetting : ToggledFeature {
    public override ref bool IsEnabled => ref Settings.SearchDescriptions;
    [LocalizedString("ToyBox_Features_SettingsFeatures_BrowserSettings_SearchDescriptionsSetting_Name", "Search Descriptions")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_BrowserSettings_SearchDescriptionsSetting_Description", "Also search through the descriptions of blueprints")]
    public override partial string Description { get; }
}
