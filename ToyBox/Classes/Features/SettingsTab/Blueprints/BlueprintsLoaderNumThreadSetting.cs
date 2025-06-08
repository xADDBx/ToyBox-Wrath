namespace ToyBox.Features.SettingsFeatures.Blueprints;
public partial class BlueprintsLoaderNumThreadSetting : FeatureWithIntSlider {
    [LocalizedString("ToyBox_Features_SettingsFeatures_Blueprints_BlueprintsLoaderNumThreadSetting_Name", "Amount of Threads")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_Blueprints_BlueprintsLoaderNumThreadSetting_Description", "This affects the amount of threads that will be used to simultaneously load blueprints. Larger is not necessarily better.")]
    public override partial string Description { get; }
    public override bool IsEnabled => true;
    public override ref int Value => ref Settings.BlueprintsLoaderNumThreads;
    public override int Min => 1;
    public override int Max => 64;
    public override int? Default => 4;
}
