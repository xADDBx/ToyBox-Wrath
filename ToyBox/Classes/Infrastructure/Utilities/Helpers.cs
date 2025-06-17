using Kingmaker;
using UnityEngine;

namespace ToyBox.Infrastructure.Utilities;
public static class Helpers {
    public static bool IsInGame() {
        return Game.Instance.Player?.Party?.Count > 0;
    }
    public static float CalculateLargestLabelSize(IEnumerable<string> items, GUIStyle? style = null) {
        style ??= GUI.skin.label;
        return items.Max(item => style.CalcSize(new(item)).x);
    }
}
