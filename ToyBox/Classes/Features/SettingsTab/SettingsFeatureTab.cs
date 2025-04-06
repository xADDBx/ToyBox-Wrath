using ToyBox.Features.SettingsFeatures.BlueprintLoaderSettings;
using ToyBox.Features.SettingsFeatures.UpdateAndIntegrity;

namespace ToyBox.Features.SettingsFeatures;
public partial class SettingsFeaturesTab : FeatureTab {
    [LocalizedString("ToyBox_Features_SettingsFeatures_SettingsFeaturesTab_SettingsText", "Settings")]
    public override partial string Name { get; }
    public SettingsFeaturesTab() {
        AddFeature(new UpdaterFeature(), "Update");
        AddFeature(new IntegrityCheckerFeature(), "Version and File Integrity");
        AddFeature(new VersionCompatabilityFeature(), "Version and File Integrity");
        AddFeature(new PerformanceEnhancementFeatures(), "Blueprints");
        AddFeature(new LanguagePickerFeature(), "Locale");
    }
}
