using Kingmaker.Blueprints;

namespace ToyBox.Infrastructure.Utilities;
public static partial class ContextProvider {
    private static bool m_ShowBlueprintBrowser = false;
    public static bool Blueprint<T>(out T? bp) where T : SimpleBlueprint {
        BlueprintPicker<T>.OnPickerGUI();
        bp = BlueprintPicker<T>.CurrentBlueprint;
        return bp != null;
    }
}
