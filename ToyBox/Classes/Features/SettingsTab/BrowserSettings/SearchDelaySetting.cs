namespace ToyBox.Features.SettingsFeatures.BrowserSettings;

public partial class SearchDelaySetting : FeatureWithFloatSlider {
    public override bool IsEnabled => Settings.ToggleSearchAsYouType; 
    public override ref float Value => ref Settings.SearchDelay;
    public override bool ShouldHide => !Settings.ToggleSearchAsYouType;
    public override float Min => 0f;
    public override float Max => 5f;
    public override float? Default => 0.3f;
    [LocalizedString("ToyBox_Features_SettingsFeatures_BrowserSettings_SearchDelaySetting_Name", "Search Delay")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_BrowserSettings_SearchDelaySetting_Description", "This is the time in seconds that is waited before a new search is automatically started")]
    public override partial string Description { get; }
}
