namespace ToyBox.Infrastructure.UI;

/// <summary>
/// A vertical paginated UI list for displaying and interacting with a collection of items of type <typeparamref name="T"/>.
/// Supports optional detail toggling and customizable pagination settings.
/// </summary>
/// <typeparam name="T">The type of items to display. Must be non-nullable.</typeparam>
public partial class VerticalList<T> where T : notnull {
#warning TODO: put into setting
    /// <summary>
    /// The maximum number of items to show per page.
    /// </summary>
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
    /// <summary>
    /// Initializes a new instance of the <see cref="VerticalList{T}"/> class.
    /// </summary>
    /// <param name="initialItems">
    /// Optional initial collection of items to populate the browser with.
    /// <para>
    /// If null, the browser starts empty until <see cref="RegisterShowAllItems"/> or
    /// <see cref="VerticalList{T}.QueueUpdateItems(IEnumerable{T}, int?)"/> is called.
    /// </para>
    /// </param>
    /// <param name="showDivBetweenItems">Whether to draw a divider between items in the list.</param>
    /// <param name="overridePageWidth">Optional override for the width of the list. Default width is 600.</param>
    /// <param name="overridePageLimit">Optional override for the number of items per page. Default PageLimit is a setting with 25 as initial value.</param>
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
    /// <summary>
    /// Clears all expanded detail sections.
    /// </summary>
    public void ClearDetails() => ToggledDetailGUIs.Clear();
    /// <summary>
    /// Toggles a collapsible detail section for the specified item.
    /// </summary>
    /// <param name="target">The item associated with the detail section.</param>
    /// <param name="key">A unique key identifying the detail section (can be different from the item itself). It's an object because T can be a struct, but structs can't be used as keys.</param>
    /// <param name="title">Optional title displayed on the disclosure toggle.</param>
    /// <param name="width">Optional width for the toggle control.</param>
    /// <returns><c>true</c> if the toggle changed state; otherwise, <c>false</c>.</returns>
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
    /// <summary>
    /// Queues an update to replace the current item list with a new collection. Runs on the main thread.
    /// </summary>
    /// <param name="newItems">The new items to display.</param>
    /// <param name="forcePage">If provided, forces the list to jump to the specified page after update.</param>
    public virtual void QueueUpdateItems(IEnumerable<T> newItems, int? forcePage = null) {
        Main.ScheduleForMainThread(new(() => {
            UpdateItems(newItems, forcePage);
        }));
    }
    /// <summary>
    /// Runs an update to replace the current item list with a new collection. Prefer the usage of <see cref="QueueUpdateItems(IEnumerable{T}, int?)"./>
    /// </summary>
    /// <param name="newItems">The new items to display.</param>
    /// <param name="forcePage">If provided, forces the list to jump to the specified page after update.</param>
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
    protected virtual void UpdatePagedItems() {
        var offset = Math.Min(ItemCount, (CurrentPage - 1) * PageLimit);
        PagedItemsCount = Math.Min(PageLimit, ItemCount - offset);
        PagedItems = Items.Skip(offset).Take(PagedItemsCount);
    }
    protected void PageGUI() {
        using (HorizontalScope()) {
            UI.Label($"{PageText.Orange()}: {CurrentPage.ToString().Cyan()} / {Math.Max(1, TotalPages).ToString().Cyan()}");
            if (TotalPages > 1) {
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
    protected virtual void HeaderGUI() => PageGUI();
    /// <summary>
    /// Renders the paged list using the provided item GUI rendering callback.
    /// </summary>
    /// <param name="onItemGUI">A delegate that renders an individual item of type <typeparamref name="T"/>.</param>
    public virtual void OnGUI(Action<T> onItemGUI) {
        using (HorizontalScope(PageWidth)) {            using (VerticalScope()) {                HeaderGUI();                foreach (var item in PagedItems) {                    if (ShowDivBetweenItems) {                        Div.DrawDiv();                    }                    onItemGUI(item);                }            }
        }
    }
    /// <summary>
    /// Renders a detail panel for an item if it is currently expanded.
    /// </summary>
    /// <param name="key">The key identifying the detail section.</param>
    /// <param name="onDetailGUI">The delegate that renders the detail UI for the item.</param>
    /// <returns><c>true</c> if the detail panel was rendered; otherwise, <c>false</c>.</returns>
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
