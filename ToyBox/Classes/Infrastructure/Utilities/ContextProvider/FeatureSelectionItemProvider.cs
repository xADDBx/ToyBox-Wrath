using Kingmaker.Blueprints.Classes.Selection;
using UnityEngine;

namespace ToyBox.Infrastructure.Utilities;
public static partial class ContextProvider {
    private static readonly TimedCache<float> m_ButtonWidth = new(() => CalculateLargestLabelSize([PickItemText], GUI.skin.button));
    private static IFeatureSelection? m_FeatureSelectionItemProviderShown = null;
    private static Browser<IFeatureSelectionItem> m_FeatureSelectionItemBrowser = new(i => BPHelper.GetSortKey(i.Feature) + BPHelper.GetFeatureSelectionParamDescription(i.Param), 
        i => BPHelper.GetSearchKey(i.Feature) + BPHelper.GetFeatureSelectionParamDescription(i.Param));
    private static Dictionary<IFeatureSelection, IFeatureSelectionItem> m_FeatureSelectionItemsCache = [];
    public static bool FeatureSelectionItemProvider<T>(T? data, out IFeatureSelectionItem? currentItem) where T : IFeatureSelection {
        currentItem = null;
        var a = PickItemText;
        using (VerticalScope()) {
            if (data == null) {
                UI.Label("Please select a Feature Selection first!".Orange());
            } else {
                m_FeatureSelectionItemsCache.TryGetValue(data, out currentItem);
                string str;
                if (currentItem != null) {
                    str = ": " + $"{BPHelper.GetTitle(currentItem.Feature)}".Green() + $" ({BPHelper.GetFeatureSelectionParamDescription(currentItem.Param)})".Cyan().Bold();
                } else {
                    str = ": " + SharedStrings.NoneText.Red();
                }
                bool isShown = m_FeatureSelectionItemProviderShown == (data as IFeatureSelection);
                if (UI.DisclosureToggle(ref isShown, SharedStrings.CurrentlySelectedBlueprintText + str)) {
                    if (isShown) {
                        m_FeatureSelectionItemProviderShown = data;
                        m_FeatureSelectionItemBrowser.UpdateItems(data.Items);
                    } else {
                        m_FeatureSelectionItemProviderShown = null;
                        m_FeatureSelectionItemBrowser.UpdateItems([]);
                    }
                }
                if (isShown) {
                    var tmp = currentItem;
                    m_FeatureSelectionItemBrowser.OnGUI(item => {
                        using (HorizontalScope()) {
                            var title = $"{BPHelper.GetTitle(item.Feature)} ({BPHelper.GetFeatureSelectionParamDescription(item.Param)})";
                            if (item != tmp) {
                                UI.Button(PickItemText, () => {
                                    tmp = item;
                                    m_FeatureSelectionItemsCache[data] = tmp;
                                    m_FeatureSelectionItemProviderShown = null;
                                }, null, Width(m_ButtonWidth));
                                title = title.Cyan().Bold();
                            } else {
                                Space(m_ButtonWidth + GUI.skin.button.margin.horizontal);
                                title = title.Orange().Bold();
                            }
                            UI.Label(title);
                        }
                    });
                    currentItem = tmp;
                }
            }
        }
        return currentItem != null;
    }
    [LocalizedString("ToyBox_Infrastructure_Utilities_FeatureSelectionItemProvider_PickItemText", "Pick Item")]
    private static partial string PickItemText { get; }
}
