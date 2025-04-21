namespace ToyBox.Infrastructure.UI;

public partial class VerticalList<T> where T : notnull {
#warning TODO: put into setting
    public int PageLimit = 25;
    protected int PageWidth = 600;
    protected int CurrentPage = 1;
    protected int PagedItemsCount = 0;
    protected int TotalPages = 1;
    protected int ItemCount = 0;
    protected bool ShowDivBetweenItems = true;
    protected readonly Dictionary<object, T> ToggledDetailGUIs = new();
    protected IEnumerable<T> PagedItems = new List<T>();
    protected IEnumerable<T> Items = new List<T>();
    public VerticalList(IEnumerable<T>? initialItems = null, bool showDivBetweenItems = true, int? overridePageWidth = null, int? overridePageLimit = null) {
        if (overridePageWidth.HasValue) {
            PageWidth = overridePageWidth.Value;
        }
        if (overridePageLimit.HasValue) {
            PageLimit = overridePageLimit.Value;
        }
        if (initialItems != null) {
            QueueUpdateItems(initialItems);
        }
        ShowDivBetweenItems = showDivBetweenItems;
    }
    public void ClearDetails() => ToggledDetailGUIs.Clear();
    // Key is an object because T can be a struct and structs can't be used as keys
    public bool DetailToggle(T target, object key, string? title = null, int width = 400) {
        var changed = false;
        if (key == null) key = target;
        var expanded = ToggledDetailGUIs.ContainsKey(key);
        if (UI.DisclosureToggle(ref expanded, title, Width(width))) {
            changed = true;
            if (expanded) {
                ToggledDetailGUIs[key] = target;
            }
        }
        return changed;
    }
    public virtual void QueueUpdateItems(IEnumerable<T> newItems, int? forcePage = null) {
        Main.ScheduleForMainThread(new(() => {
            UpdateItems(newItems, forcePage);
        }));
    }
    internal virtual void UpdateItems(IEnumerable<T> newItems, int? forcePage = null) {
        if (forcePage != null) {
            CurrentPage = 1;
        }
        Items = newItems;
        ItemCount = Items.Count();
        if (PageLimit > 0) {
            TotalPages = (int)Math.Ceiling((double)ItemCount / PageLimit);
            CurrentPage = Math.Max(Math.Min(CurrentPage, TotalPages), 1);
        } else {
            CurrentPage = 1;
            TotalPages = 1;
        }
        UpdatePagedItems();
    }
    public virtual void UpdatePagedItems() {
        var offset = Math.Min(ItemCount, (CurrentPage - 1) * PageLimit);
        PagedItemsCount = Math.Min(PageLimit, ItemCount - offset);
        PagedItems = Items.Skip(offset).Take(PagedItemsCount);
    }
    protected void PageGUI() {
        using (HorizontalScope()) {
            if (TotalPages > 1) {
                UI.Label($"{PageText.Orange()}: {CurrentPage.ToString().Cyan()} / {TotalPages.ToString().Cyan()}");
                Space(25);
                if (UI.Button("-")) {
                    if (CurrentPage <= 1) {
                        CurrentPage = TotalPages;
                    } else {
                        CurrentPage -= 1;
                    }
                    UpdatePagedItems();
                }
                if (UI.Button("+")) {
                    if (CurrentPage >= TotalPages) {
                        CurrentPage = 1;
                    } else {
                        CurrentPage += 1;
                    }
                    UpdatePagedItems();
                }
            }
        }
    }
    public virtual void HeaderGUI() => PageGUI();
    public virtual void OnGUI(Action<T> onItemGUI) {
        using (VerticalScope(PageWidth)) {
            HeaderGUI();
            foreach (var item in PagedItems) {
                if (ShowDivBetweenItems) {
                    Div.DrawDiv();
                }
                onItemGUI(item);
            }
        }
    }
    public bool DetailGUI(object key, Action<T> onDetailGUI) {
        ToggledDetailGUIs.TryGetValue(key, out var target);
        if (target != null) {
            onDetailGUI(target);
            return true;
        } else {
            return false;
        }
    }

    [LocalizedString("ToyBox_Infrastructure_UI_VerticalList_PageText", "Page")]
    private static partial string PageText { get; }
}
