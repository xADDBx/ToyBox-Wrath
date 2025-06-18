using ToyBox.Features.SettingsFeatures.Blueprints;
using ToyBox.Features.SettingsFeatures.BrowserSettings;
using ToyBox.Features.SettingsFeatures.UpdateAndIntegrity;
using ToyBox.Features.SettingsTab.Inspector;

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
    [LocalizedString("ToyBox_Features_SettingsFeatures_SettingsFeaturesTab_InspectorText", "Inspector")]
    private static partial string InspectorText { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_SettingsFeaturesTab_OtherText", "Other")]
    private static partial string OtherText { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_SettingsFeaturesTab_SettingsText", "Settings")]
    public override partial string Name { get; }
    public SettingsFeaturesTab() {
        AddFeature(new UpdaterFeature(), UpdateText);

        AddFeature(new PageLimitSetting(), ListsAndBrowsersText);
        AddFeature(new SearchAsYouTypeFeature(), ListsAndBrowsersText);
        AddFeature(new SearchDelaySetting(), ListsAndBrowsersText);

        AddFeature(new IntegrityCheckerFeature(), VersionAndFileIntegrityText);
        AddFeature(new VersionCompatabilityFeature(), VersionAndFileIntegrityText);

        AddFeature(new PerformanceEnhancementFeatures(), BlueprintsText);
        AddFeature(new PreloadBlueprintsFeature(), BlueprintsText);
        AddFeature(new ShowDisplayAndInternalNamesSetting(), BlueprintsText);
        AddFeature(new BlueprintsLoaderNumThreadSetting(), BlueprintsText);
        AddFeature(new BlueprintsLoaderNumShardSetting(), BlueprintsText);
        AddFeature(new BlueprintsLoaderChunkSizeSetting(), BlueprintsText);

        AddFeature(new InspectorShowNullAndEmptyMembersSetting(), InspectorText);
        AddFeature(new InspectorShowEnumerableFieldsSetting(), InspectorText);
        AddFeature(new InspectorShowStaticMembersSetting(), InspectorText);
        AddFeature(new InspectorIndentWidthSetting(), InspectorText);
        AddFeature(new InspectorNameFractionOfWidthSetting(), InspectorText);

        AddFeature(new LogLevelSetting(), OtherText);

        AddFeature(new LanguagePickerFeature(), LanguageText);
    }

}
