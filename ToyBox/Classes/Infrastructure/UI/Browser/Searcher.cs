using System.Collections.Concurrent;
using System.Diagnostics;
using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public class ThreadedListSearcher<T> where T: notnull {
    private float m_LastSharedResults = 0f;
    private const float m_ShareResultsDelay = 0.05f;
    private const int m_MaxNumForPartialUpdate = 200;
    public bool IsRunning = false;
    public int CurrentlyFound;
    private readonly VerticalList<T> m_Parent;
    private CancellationTokenSource? m_SearchCts;
    public ConcurrentQueue<T>? m_InProgress;
    public ThreadedListSearcher(VerticalList<T> parent) {
        m_Parent = parent;
    }
    public void StartSearch(IEnumerable<T> items, string query, Func<T, string> getSearchKey, Func<T, string> getSortKey) {
        lock (this) {
            if (IsRunning) {
                StopSearch();
            }
        }
        m_SearchCts = new();
        Task.Run(() => DoSearch(items, query, getSearchKey, getSortKey, m_SearchCts.Token, m_SearchCts));
    }
    private void DoSearch(IEnumerable<T> items, string query, Func<T, string> getSearchKey, Func<T, string> getSortKey, CancellationToken ct, CancellationTokenSource cts) {
        lock (this) {
            IsRunning = true;
        }
        try {
            var watch = Stopwatch.StartNew();
            m_LastSharedResults = Time.time;
            Debug("Start Search");
            var allResults = new List<T>();
            m_InProgress = new();
            CurrentlyFound = 0;
            var lastShared = 0;
            if (!string.IsNullOrEmpty(query)) {
                var terms = query.Split(' ').Select(s => s.ToUpper());
                foreach (var item in items) {
                    if (ct.IsCancellationRequested) {
                        lock (this) {
                            IsRunning = false;
                            cts.Dispose();
                        }
                        m_Parent.QueueUpdateItems(allResults.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).OrderBy(getSortKey).ToArray(), 1);
                        Debug("Cancelled Search");
                        return;
                    }
                    var text = getSearchKey(item);
                    if (terms.All(text.Contains)) {
                        allResults.Add(item);
                        m_InProgress.Enqueue(item);
                        CurrentlyFound++;
                        if (lastShared < m_MaxNumForPartialUpdate && (Time.time - m_LastSharedResults) > m_ShareResultsDelay) {
                            lastShared = CurrentlyFound;
                            m_LastSharedResults = Time.time;
                            Main.ScheduleForMainThread(() => {
                                m_Parent.UpdateItems([.. m_InProgress], 1);
                            });
                        }
                    }
                }
                m_Parent.QueueUpdateItems(allResults.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).OrderBy(getSortKey).ToArray(), 1);
            } else {
                m_Parent.QueueUpdateItems(items.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).OrderBy(getSortKey).ToArray(), 1);
            }
            Debug($"Searched {items.Count()} items in {watch.ElapsedMilliseconds}ms; found {allResults.Count} results");
        } catch (Exception e) {
            Error($"Encountered exception while trying to search!\n{e}");
        }
        lock (this) {
            IsRunning = false;
            cts.Dispose();
        }
    }
    public void StopSearch() {
        m_SearchCts?.Cancel();
    }
}
