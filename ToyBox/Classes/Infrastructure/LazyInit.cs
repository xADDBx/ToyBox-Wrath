using Kingmaker;
using System.Diagnostics;

namespace ToyBox.Infrastructure;
public static class LazyInit {
    internal static Stopwatch Stopwatch = new();
    public static void EnsureFinish() {
        var original = AccessTools.Method(typeof(MainMenu), nameof(MainMenu.Awake));
        var patch = AccessTools.Method(typeof(LazyInit), nameof(LazyInit.MainMenu_Awake_Postfix));
        Main.HarmonyInstance.Patch(original, postfix: new(patch));
    }
    public static void MainMenu_Awake_Postfix() {
        Debug($"Lazy init had {Stopwatch.ElapsedMilliseconds}ms before waiting");
        Stopwatch sw = Stopwatch.StartNew();
        Task.WaitAll(Main.LateInitTasks.ToArray());
        Debug($"Waited {sw.ElapsedMilliseconds}ms for lazy init finish");
    }
}
