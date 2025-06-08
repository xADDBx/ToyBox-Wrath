namespace ToyBox.Features.SettingsFeatures.Blueprints;

public partial class BlueprintsLoaderChunkSizeSetting : FeatureWithIntSlider {
    [LocalizedString("ToyBox_Features_SettingsFeatures_Blueprints_BlueprintsLoaderChunkSizeSetting_Name", "Chunk Size")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_Blueprints_BlueprintsLoaderChunkSizeSetting_Description", "Affects the amount of blueprints a thread loads at once. A lower number means better load balancing but more synchronization overhead.")]
    public override partial string Description { get; }
    public override bool IsEnabled => true;
    public override ref int Value => ref Settings.BlueprintsLoaderChunkSize;
    public override int Min => 1;
    public override int Max => 250000;
    public override int? Default => 200;
}
