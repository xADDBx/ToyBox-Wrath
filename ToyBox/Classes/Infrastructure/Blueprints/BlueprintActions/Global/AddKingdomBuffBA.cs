using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class AddKingdomBuffBA : BlueprintActionFeature, IBlueprintAction<BlueprintKingdomBuff> {
    public bool CanExecute(BlueprintKingdomBuff blueprint, params object[] parameter) {
        return KingdomState.Instance != null
            && !KingdomState.Instance.ActiveBuffs.HasFact(blueprint);
    }
    private bool Execute(BlueprintKingdomBuff blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        KingdomState.Instance.AddBuff(blueprint, null, null, 0);
        return true;
    }
    public bool? OnGui(BlueprintKingdomBuff blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(StyleActionString(AddText, isFeatureSearch), () => {
                result = Execute(blueprint, parameter);
            });
        } else if (isFeatureSearch) {
            if (KingdomState.Instance != null) {
                UI.Label(KingdomAlreadyHasThisBuffText.Red().Bold());
            } else {
                UI.Label(ThereCurrentlyExistsNoKingdomTex.Red().Bold());
            }
        }
        return result;
    }
    public bool GetContext(out BlueprintKingdomBuff? context) => ContextProvider.Blueprint(out context);
    public override void OnGui() {
        if (GetContext(out var bp)) {
            OnGui(bp!, true);
        }
    }

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddKingdomBuffBA_AddText", "Add")]
    private static partial string AddText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddKingdomBuffBA_Name", "Add Kingdom Buff")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddKingdomBuffBA_Description", "Adds the specified BlueprintKingdomBuff to the chosen kingdom.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddKingdomBuffBA_KingdomAlreadyHasThisBuffText", "Kingdom already has this Buff")]
    private static partial string KingdomAlreadyHasThisBuffText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_AddKingdomBuffBA_ThereCurrentlyExistsNoKingdomTex", "There currently exists no kingdom")]
    private static partial string ThereCurrentlyExistsNoKingdomTex { get; }
}
