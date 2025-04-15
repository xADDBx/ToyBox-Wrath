using System.Collections;
using System.Runtime.CompilerServices;
using UniRx;
using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public delegate void OutEnumerableAction<T>(out IEnumerable<T> result) where T : notnull;
public partial class Browser<T> : VerticalList<T> where T: notnull {
#warning TODO: Put in Settings
    protected bool SearchAsYouType = true;
    protected const float SearchDelay = 0.3f;
    protected float LastSearchedAt = 0f;
    protected string CurrentSearchString = "";
    public string LastSearchedFor = "";
    private string? m_SearchBarControlName;
    protected OutEnumerableAction<T>? ShowAllFunc = null;
    private bool m_ShowAllFuncCalled = false;
    protected bool ShowAll = false;
    protected IEnumerable<T>? UnsearchedShowAllItems = null;
    protected IEnumerable<T> UnsearchedItems = new List<T>();
    protected Func<T, string> GetSearchKey;
    protected Func<T, string> GetSortKey;
    public Browser(Func<T, string> sortKey, Func<T, string> searchKey, IEnumerable<T>? initialItems = null, OutEnumerableAction<T>? showAllFunc = null, bool showDivBetweenItems = true, int? overridePageWidth = null, int? overridePageLimit = null) 
        : base(initialItems, showDivBetweenItems, overridePageWidth, overridePageLimit) {
        GetSearchKey = searchKey;
        GetSortKey = sortKey;
    }
    public void StartNewSearch(string query, bool force = false) {
        if (!force && LastSearchedFor == query) return;
        LastSearchedFor = query;
        LastSearchedAt = Time.time;
#warning TODO: Start search
        /*
        if (Searcher.IsSearching) {
            Searcher.CancellationThingy
        }
        Searcher.StartNewSearchThingy(query, ShowAll ? UnsearchedShowAllItems : UnsearchedItems);
        */
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
                        if (UnsearchedShowAllItems == null && newValue && !m_ShowAllFuncCalled) {
                            m_ShowAllFuncCalled = true;
                            ShowAllFunc(out UnsearchedShowAllItems);
                        } else {
                            StartNewSearch(CurrentSearchString, true);
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
