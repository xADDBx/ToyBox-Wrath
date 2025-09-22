using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Persistence;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class LoadAreaPresetBA : IBlueprintAction<BlueprintAreaPreset> {
    static LoadAreaPresetBA() {
        BlueprintActions.RegisterAction((IBlueprintAction<SimpleBlueprint>)new LoadAreaPresetBA());
    }
    private bool CanExecute(BlueprintAreaPreset blueprint, params object[] parameter) {
        return true;
    }
    private bool Execute(BlueprintAreaPreset blueprint, params object[] parameter) {
        ((IBlueprintAction<SimpleBlueprint>)this).LogBPAction(blueprint, parameter);
        LoadingProcess.Instance.StartCoroutine(CheatsTransfer.NewGameCoroutine(blueprint));
        return true;
    }
    public bool? OnGui(BlueprintAreaPreset blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(LoadPresetText, () => {
                 result = Execute(blueprint, parameter);
            });
        }
        return result;
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_LoadAreaPresetBA_LoadPresetText", "Load Preset")]
    private static partial string LoadPresetText { get; }
}
