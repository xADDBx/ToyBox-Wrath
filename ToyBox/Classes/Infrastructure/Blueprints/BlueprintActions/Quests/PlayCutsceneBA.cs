using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public partial class PlayCutsceneBA : BlueprintActionFeature, IBlueprintAction<Cutscene> {

    private bool CanExecute(Cutscene blueprint) {
        return IsInGame();
    }
    private bool Execute(Cutscene blueprint) {
        LogExecution(blueprint);
        ToggleModWindow();
        
        var cutscenePlayerData = CutscenePlayerData.Queue.FirstOrDefault(c => c.PlayActionId == blueprint.name);
        if (cutscenePlayerData != null) {
            cutscenePlayerData.PreventDestruction = true;
            cutscenePlayerData.Stop();
            cutscenePlayerData.PreventDestruction = false;
        }
        var state = ContextData<SpawnedUnitData>.Current?.State;
        CutscenePlayerView.Play(blueprint, null, true, state).PlayerData.PlayActionId = blueprint.name;

        return true;
    }
    public bool? OnGui(Cutscene blueprint, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint)) {
            UI.Button(PlayText, () => {
                result = Execute(blueprint);
            });
        }
        return result;
    }

    public bool GetContext(out Cutscene? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!);
        }
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_PlayCutsceneBA_Name", "Play Cutscene")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_PlayCutsceneBA_Description", "Plays the specified Cutscene.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_PlayCutsceneBA_PlayText", "Play")]
    private static partial string PlayText { get; }
}
