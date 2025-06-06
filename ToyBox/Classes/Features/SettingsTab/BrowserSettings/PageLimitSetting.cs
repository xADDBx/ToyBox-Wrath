namespace ToyBox.Features.SettingsFeatures.BrowserSettings;

public partial class PageLimitSetting : FeatureWithIntSlider {
    public override bool IsEnabled => true;
    public override ref int Value => ref Settings.PageLimit;
    public override int Min => 1;
    public override int Max => 400;
    public override int? Default => 25;
    protected override void OnValueChanged((int oldValue, int newValue) vals) {
        base.OnValueChanged(vals);
        List<WeakReference<IPagedList>> newList = [];
        foreach (var maybeVl in Main.m_VerticalLists) {
            if (maybeVl?.TryGetTarget(out var pagedList) ?? false) {
                newList.Add(maybeVl);
                pagedList.UpdatePages();
            }
        }
        Main.m_VerticalLists = newList;
    }
    [LocalizedString("ToyBox_Features_SettingsFeatures_BrowserSettings_PageLimitSetting_Name", "Page Limit")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsFeatures_BrowserSettings_PageLimitSetting_Description", "Restricts the amount of items a page of a list/browser can display")]
    public override partial string Description { get; }
}
