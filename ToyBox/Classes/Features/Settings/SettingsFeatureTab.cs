using ToyBox.Features.UpdateAndIntegrity;

namespace ToyBox.Features.SettingsFeatures;
public partial class SettingsFeaturesTab : FeatureTab {
    [LocalizedString("ToyBox_Features_SettingsFeatures_SettingsFeaturesTab_SettingsText", "Settings")]
    public override partial string Name { get; }
    public SettingsFeaturesTab() {
        AddFeature(new UpdateAndIntegrityFeature());
        AddFeature(new PerformanceEnhancementFeatures());
        AddFeature(new LanguagePickerFeature());
    }
    public override void OnGui() {
        Updater.UpdaterGUI(Main.ModEntry);
        base.OnGui();
    }
}
