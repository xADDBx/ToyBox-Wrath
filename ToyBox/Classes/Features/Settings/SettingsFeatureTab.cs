namespace ToyBox.Features.SettingsFeature;
public class SettingsFeatureTab : FeatureTab {
    [LocalizedString("Features.Settings.SettingsFeatureTab.Name")]
    private static string m_Name = "Settings";
    public override string Name => m_Name;
    public SettingsFeatureTab() {
        Features.Add(new LanguagePickerFeature());
    }
}
