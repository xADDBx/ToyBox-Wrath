namespace ToyBox.Infrastructure.UI;
public static partial class UI {
    static UI() {
        Main.OnLocaleChanged += ClearLocaleCaches;
        Main.OnHideGUIAction += ClearHideCaches;
    }
    private static void ClearLocaleCaches() {
        m_EnumCache.Clear();
        m_IndexToEnumCache.Clear();
        m_EnumNameCache.Clear();
    }
    private static void ClearHideCaches() {
        m_EditStateCaches.Clear();
    }
}
