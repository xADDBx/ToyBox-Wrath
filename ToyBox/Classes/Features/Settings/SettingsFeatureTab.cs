namespace ToyBox.Features.SettingsFeature;
public partial class SettingsFeatureTab : FeatureTab {
    [LocalizedString("Features_Settings_SettingsFeatureTab_Name", "Settings")]
    public override partial string Name { get; }
    public SettingsFeatureTab() {
        Features.Add(new LanguagePickerFeature());
    }
}
