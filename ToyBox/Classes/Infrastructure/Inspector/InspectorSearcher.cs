using System.Collections;
using System.Diagnostics;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Inspector;
public static class InspectorSearcher {
    private static object m_SyncRoot = new();
    internal static bool IsRunning {
        get;
        private set;
    } = false;
    public static string? LastPrompt = "";
    private static Stopwatch? m_Stopwatch;
    public static bool m_ShouldCancel = false;
    public static bool DidSearch {
        get {
            return LastPrompt != "" && !IsRunning;
        }
    }
    public enum SearchMode {
        ValueSearch,
        TypeSearch,
        NameSearch
    }
    public static void StartSearch(SearchMode mode, InspectorNode root, int depthToSearch, string query) {
        lock (m_SyncRoot) {
            if (query.ToUpper() != LastPrompt && !IsRunning) {
                // We can't use a thread to run this sadly
                // This should probably be further Coroutin-ed; making yields more granular and reducing UI lag
                ToyBoxBehaviour.Instance.StartCoroutine(SearchCoroutine(mode, root, depthToSearch, query));
                IsRunning = true;
                m_ShouldCancel = false;
                LastPrompt = query.ToUpper();
            }
        }
        m_Stopwatch = Stopwatch.StartNew();
    }
    private static IEnumerator SearchCoroutine(SearchMode mode, InspectorNode root, int depthToSearch, string query) {
        var work = new Stack<(InspectorNode node, int depth)>();
        foreach (var child in root.Children) {
            work.Push((child, depthToSearch));
        }

        int processed = 0;
        while (work.Count > 0) {
            var (node, depth) = work.Pop();
            node.IsSelfMatched = MatchNode(mode, node, query);
            node.Parent!.IsChildMatched |= node.IsMatched;
            if (depth > 0) {
                foreach (var c in node.Children) {
                    work.Push((c, depth - 1));
                }
            }
            // every 50 nodes, give the GUI a frame
            if (++processed % Settings.InspectorSearchBatchSize == 0) {
                if (m_ShouldCancel) {
                    lock (m_SyncRoot) {
                        LastPrompt = "";
                        IsRunning = false;
                        m_ShouldCancel = false;
                    }
                    Debug($"Inspector Search aborted after  {m_Stopwatch?.ElapsedMilliseconds.ToString() ?? "??????? Something is seriously wrong "}ms");
                    yield break;
                }
                yield return null;
            }
        }
        lock (m_SyncRoot) {
            IsRunning = false;
        }
        Debug($"Inspector Search finished in {m_Stopwatch?.ElapsedMilliseconds.ToString() ?? "??????? Something is seriously wrong "}ms");
    }
    private static bool MatchNode(SearchMode mode, InspectorNode node, string query) {
        if (mode == SearchMode.ValueSearch) {
            return MatchString(node.ValueText, query);
        } else if (mode == SearchMode.NameSearch) {
            return MatchString(node.NameText, query);
        } else if (mode == SearchMode.TypeSearch) {
            return MatchString(node.TypeNameText, query);
        }
        return false;
    }
    private static bool MatchString(string text, string query) {
        if (!string.IsNullOrEmpty(query)) {
            var terms = query.Split(' ').Select(s => s.ToUpper());
            text = text.ToUpper();
            return terms.All(text.Contains);
        } else {
            return false;
        }
    }
}
