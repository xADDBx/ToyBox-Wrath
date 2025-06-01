using Kingmaker;

namespace ToyBox.Infrastructure.Utilities;
public static class Helpers {
    public static bool IsInGame() {
        return Game.Instance.Player?.Party?.Count > 0;
    }
}
