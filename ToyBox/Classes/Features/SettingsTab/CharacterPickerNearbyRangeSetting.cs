namespace ToyBox.Features.SettingsFeatures;

public partial class CharacterPickerNearbyRangeSetting : FeatureWithFloatSlider {
    [LocalizedString("ToyBox_Features_SettingsFeatures_CharacterPickerNearbyRangeSetting_Name", "Nearby Range")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_CharacterPickerNearbyRangeSetting_Description", "Modifies the range for the Nearby Character Picker category")]
    public override partial string Description { get; }
    public override bool IsEnabled => true;

    public override ref float Value => ref Settings.NearbyRange;

    public override float Min => 1f;

    public override float Max => 100000f;

    public override float? Default => 25f;
}
