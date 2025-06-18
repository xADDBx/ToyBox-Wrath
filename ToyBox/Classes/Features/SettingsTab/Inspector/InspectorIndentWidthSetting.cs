namespace ToyBox.Features.SettingsTab.Inspector;
public partial class InspectorIndentWidthSetting : FeatureWithFloatSlider {
    public override bool IsEnabled => true;
    public override ref float Value => ref Settings.InspectorIndentWidth;
    public override float Min => 0f;
    public override float Max => 200f;
    public override float? Default => 20f;
    [LocalizedString("ToyBox_Features_SettingsTab_Inspector_InspectorIndentWidthSetting_Name", "Indent Width")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsTab_Inspector_InspectorIndentWidthSetting_Description", "Amount of space that is indented for each nested level in the inspector")]
    public override partial string Description { get; }
}
