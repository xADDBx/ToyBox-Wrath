using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints;
using Kingmaker.UI;
using System.Collections.Concurrent;

namespace ToyBox.Infrastructure.Blueprints;
public static class BPHelper {
    private static ConcurrentDictionary<(SimpleBlueprint, Func<string, string>), string> m_TitleCache = new();
    private static ConcurrentDictionary<SimpleBlueprint, string> m_SortKeyCache = new();
    private static ConcurrentDictionary<SimpleBlueprint, string> m_SearchKeyCache = new();
    public static string GetTitle(SimpleBlueprint blueprint, Func<string, string>? formatter = null) {
        if (formatter == null) formatter = s => s;
        if (!m_TitleCache.TryGetValue((blueprint, formatter), out var title)) {
            title = m_InternalGetTitle(blueprint, formatter);
            m_TitleCache[(blueprint, formatter)] = title;
        }
        return title;
    }
    public static string GetSortKey(SimpleBlueprint blueprint) {
        if (!m_SortKeyCache.TryGetValue(blueprint, out var sortKey)) {
            sortKey = m_InternalGetSortKey(blueprint);
            m_SortKeyCache[blueprint] = sortKey;
        }
        return sortKey;
    }
    public static string GetSearchKey(SimpleBlueprint blueprint) {
        if (!m_SearchKeyCache.TryGetValue(blueprint, out var searchKey)) {
            searchKey = m_InternalGetSearchKey(blueprint);
            m_SearchKeyCache[blueprint] = searchKey;
        }
        return searchKey;
    }
    private static string m_InternalGetTitle(SimpleBlueprint blueprint, Func<string, string> formatter) {
        string? ret = null;
        try {
            if (blueprint is IUIDataProvider uiDataProvider) {
                string Name = "";
                try {
                    Name = uiDataProvider.Name;
                } catch (Exception) {
                    Debug($"Error while getting name for {uiDataProvider}");
                }
                ret = CheckNullName(blueprint, Name);
            } else if (blueprint is BlueprintItemEnchantment enchantment) {
                ret = CheckNullName(blueprint, enchantment.Name);
            }
            ret ??= blueprint.name;
        } catch (Exception ex) {
            Debug($"Error getting SortKey for BP: {blueprint} - {blueprint.AssetGuid}");
            Debug(ex.ToString());
            ret ??= "<ToyBox Error>";
        }
        return formatter(ret);
    }
    private static string m_InternalGetSearchKey(SimpleBlueprint blueprint) {
        string? ret = null;
        try {
            if (blueprint is IUIDataProvider uiDataProvider) {
                string Name = "";
                try {
                    Name = uiDataProvider.Name;
                } catch (Exception) {
                    Debug($"Error while getting name for {uiDataProvider}");
                }
                ret = CheckNullName(blueprint, Name, true);
            } else if (blueprint is BlueprintItemEnchantment enchantment) {
                ret = CheckNullName(blueprint, enchantment.Name, true);
            }
            ret ??= blueprint.name;
        } catch (Exception ex) {
            Debug($"Error getting SearchKey for BP: {blueprint} - {blueprint.AssetGuid}");
            Debug(ex.ToString());
            ret ??= "<ToyBox Error>";
        }
        return (ret + $" {blueprint.AssetGuid} {blueprint.GetType()}").ToUpper();
    }
    private static string m_InternalGetSortKey(SimpleBlueprint blueprint) {
        string? ret = null;
        try {
            if (blueprint is IUIDataProvider uiDataProvider) {
                string Name = "";
                try {
                    Name = uiDataProvider.Name;
                } catch (Exception) {
                    Debug($"Error while getting name for {uiDataProvider}");
                }
                ret = CheckNullName(blueprint, Name);
            } else if (blueprint is BlueprintItemEnchantment enchantment) {
                ret = CheckNullName(blueprint, enchantment.Name);
            }
            ret ??= blueprint.name;
        } catch (Exception ex) {
            Debug($"Error getting SortKey for BP: {blueprint} - {blueprint.AssetGuid}");
            Debug(ex.ToString());
            ret ??= "<ToyBox Error>";
        }
        return ret;
    }
    private static string CheckNullName(SimpleBlueprint bp, string name, bool forceInternalDisplay = false, bool colorInternal = false) {
        if (string.IsNullOrEmpty(name)) return bp.name;
        if (name == "<null>" || name.StartsWith("[unknown key: ")) {
            name = colorInternal ? bp.name.DarkGrey() : bp.name;
        } else if (Settings.ToggleBPsShowDisplayAndInternalName || forceInternalDisplay) {
            name += colorInternal ? bp.name.DarkGrey() : bp.name;
        }
        return name;
    }
}
