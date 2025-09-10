namespace ToyBox.Features.SettingsTab.Inspector;

public partial class InspectorDrawLimitSetting : FeatureWithIntSlider {
    public override bool IsEnabled => true;

    public override ref int Value => ref Settings.InspectorDrawLimit;

    public override int Min => 10;

    public override int Max => 10000;

    public override int? Default => 500;
    [LocalizedString("ToyBox_Features_SettingsTab_Inspector_InspectorDrawLimitSetting_Name", "Inspector Draw Limit")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsTab_Inspector_InspectorDrawLimitSetting_Description", "Limits the amount of items/rows that are drawn to prevent freezing the ui. Items exceeding the limit are discarded.")]
    public override partial string Description { get; }
}
