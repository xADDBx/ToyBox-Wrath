namespace ToyBox.Infrastructure.UI;
public static partial class UI {
    static UI() {
        Main.OnLocaleChanged += ClearCaches;
    }
    private static void ClearCaches() {
        m_EnumCache.Clear();
        m_IndexToEnumCache.Clear();
        m_EnumNameCache.Clear();
    }
}
