using ToyBox.Features.SettingsFeatures.BlueprintLoaderSettings;
using ToyBox.Features.SettingsFeatures.BrowserSettings;
using ToyBox.Features.SettingsFeatures.UpdateAndIntegrity;

namespace ToyBox.Features.SettingsFeatures;
public partial class SettingsFeaturesTab : FeatureTab {
    [LocalizedString("ToyBox_Features_SettingsFeatures_SettingsFeaturesTab_UpdateText", "Update")]
    private static partial string UpdateText { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_SettingsFeaturesTab_VersionAndFileIntegrityCategory", "Version and File Integrity")]
    private static partial string VersionAndFileIntegrityText { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_SettingsFeaturesTab_BlueprintsCategory", "Blueprints")]
    private static partial string BlueprintsText { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_SettingsFeaturesTab_LanguageCategory", "Language")]
    private static partial string LanguageText { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_SettingsFeaturesTab_ListsAndBrowsersText", "Lists and Browsers")]
    private static partial string ListsAndBrowsersText { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_SettingsFeaturesTab_SettingsText", "Settings")]
    public override partial string Name { get; }
    public SettingsFeaturesTab() {
        AddFeature(new UpdaterFeature(), UpdateText);

        AddFeature(new PageLimitSetting(), ListsAndBrowsersText);

        AddFeature(new IntegrityCheckerFeature(), VersionAndFileIntegrityText);
        AddFeature(new VersionCompatabilityFeature(), VersionAndFileIntegrityText);
        AddFeature(new PerformanceEnhancementFeatures(), BlueprintsText);
        AddFeature(new PreloadBlueprintsFeature(), BlueprintsText);
        AddFeature(new LanguagePickerFeature(), LanguageText);
    }
}
