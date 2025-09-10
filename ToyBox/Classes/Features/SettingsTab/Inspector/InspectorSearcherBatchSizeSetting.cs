namespace ToyBox.Features.SettingsTab.Inspector;

public partial class InspectorSearcherBatchSizeSetting : FeatureWithLogIntSlider {
    public override bool IsEnabled => true;

    public override ref int Value => ref Settings.InspectorSearchBatchSize;

    public override int Min => 100;

    public override int Max => 1000000;

    public override int? Default => 20000;
    [LocalizedString("ToyBox_Features_SettingsTab_Inspector_InspectorSearcherBatchSizeSetting_Name", "Inspector Searcher Batch Size")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsTab_Inspector_InspectorSearcherBatchSizeSetting_Description", "Lower numbers mean less ui lag during search but longer search time.")]
    public override partial string Description { get; }
}
