using Kingmaker.Utility;
using System.Collections;
using System.Runtime.CompilerServices;
using UniRx;
using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public partial class Browser<T> : VerticalList<T> where T: notnull {
#warning TODO: Put in Settings
    protected bool SearchAsYouType = true;
    protected const float SearchDelay = 0.3f;
    protected float LastSearchedAt = 0f;
    protected string CurrentSearchString = "";
    public string LastSearchedFor = "";
    private string? m_SearchBarControlName;
    protected Action<Action<IEnumerable<T>>>? ShowAllFunc = null;
    private bool m_ShowAllFuncCalled = false;
    protected bool ShowAll = false;
    protected IEnumerable<T>? UnsearchedShowAllItems = null;
    protected IEnumerable<T> UnsearchedItems = new List<T>();
    protected Func<T, string> GetSearchKey;
    protected Func<T, string> GetSortKey;
    protected ThreadedListSearcher<T> Searcher;
    public Browser(Func<T, string> sortKey, Func<T, string> searchKey, IEnumerable<T>? initialItems = null, Action<Action<IEnumerable<T>>>? showAllFunc = null, bool showDivBetweenItems = true, int? overridePageWidth = null, int? overridePageLimit = null) 
        : base(initialItems, showDivBetweenItems, overridePageWidth, overridePageLimit) {
        ShowAllFunc = showAllFunc;
        GetSearchKey = searchKey;
        GetSortKey = sortKey;
        Searcher = new(this);
    }
    public void RedoSearch() => StartNewSearch(CurrentSearchString, true);
    public void RegisterShowAllItems(IEnumerable<T> items) {
        Main.ScheduleForMainThread(() => {
            UnsearchedShowAllItems = items;
            StartNewSearch(CurrentSearchString, true);
        });
    }
    public void StartNewSearch(string query, bool force = false) {
        if (!force && LastSearchedFor == query) return;
        bool canOptimizeSearch = !query.IsNullOrEmpty() && query.StartsWith(LastSearchedFor);
        LastSearchedFor = query;
        LastSearchedAt = Time.time;
        CurrentPage = 1;
        if (canOptimizeSearch) {
            Searcher.StartSearch(Items, query, GetSearchKey, GetSortKey);
        } else {
            Searcher.StartSearch((ShowAll && UnsearchedShowAllItems != null) ? UnsearchedShowAllItems : UnsearchedItems, query, GetSearchKey, GetSortKey);
        }
    }
    protected void SearchBarGUI() {
        IEnumerator DebouncedSearch() {
            yield return new WaitForSeconds(1.5f * SearchDelay);
            if (!CurrentSearchString.Equals(LastSearchedFor)) {
                StartNewSearch(CurrentSearchString);
            }
        }
        m_SearchBarControlName ??= RuntimeHelpers.GetHashCode(this).ToString();
        Action<string>? contentChangedAction = SearchAsYouType ? ((string newQuery) => {
            if (Time.time - LastSearchedAt > SearchDelay) {
                StartNewSearch(newQuery);
            } else {
                MainThreadDispatcher.StartUpdateMicroCoroutine(DebouncedSearch());
            }
        }) : null; 
        UI.ActionTextField(ref CurrentSearchString, m_SearchBarControlName, contentChangedAction, (string query) => StartNewSearch(query));
    }
    public override void HeaderGUI() {
        using (VerticalScope()) {
            using (HorizontalScope()) {
                PageGUI();
                Space(30);
                if (ShowAllFunc != null) {
                    var newValue = GUILayout.Toggle(ShowAll, ShowAllText.Cyan(), AutoWidth());
                    if (newValue != ShowAll) {
                        ShowAll = newValue;
                        if (UnsearchedShowAllItems == null && newValue && !m_ShowAllFuncCalled) {
                            m_ShowAllFuncCalled = true;
                            ShowAllFunc(RegisterShowAllItems);
                        } else {
                            RedoSearch();
                        }
                    }
                }
            }
            SearchBarGUI();
        }
    }

    [LocalizedString("ToyBox_Infrastructure_UI_Browser_ShowAllText", "Show All")]
    private static partial string ShowAllText { get; }
}
