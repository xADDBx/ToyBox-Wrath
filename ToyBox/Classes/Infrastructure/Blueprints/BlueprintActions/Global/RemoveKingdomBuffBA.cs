using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class RemoveKingdomBuffBA : BlueprintActionFeature, IBlueprintAction<BlueprintKingdomBuff> {
    private bool CanExecute(BlueprintKingdomBuff blueprint, params object[] parameter) {
        return KingdomState.Instance != null && KingdomState.Instance.ActiveBuffs.HasFact(blueprint);
    }
    private bool Execute(BlueprintKingdomBuff blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        KingdomState.Instance.ActiveBuffs.RemoveFact(blueprint);
        return true;
    }
    public bool? OnGui(BlueprintKingdomBuff blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, parameter)) {
            UI.Button(StyleActionString(RemoveText, isFeatureSearch), () => {
                result = Execute(blueprint, parameter);
            });
        } else if (isFeatureSearch) {
            if (KingdomState.Instance != null) {
                UI.Label(KingdomDoesNotHaveThisBuff.Red().Bold());
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

    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveKingdomBuffBA_RemoveText", "Remove")]
    private static partial string RemoveText { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveKingdomBuffBA_Name", "Remove Kingdom Buff")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveKingdomBuffBA_Description", "Removes the specified BlueprintKingdomBuff from the chosen kingdom.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveKingdomBuffBA_KingdomDoesNotHaveThisBuffText", "Kingdom does not have this buff")]
    private static partial string KingdomDoesNotHaveThisBuff { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_RemoveKingdomBuffBA_ThereCurrentlyExistsNoKingdomTex", "There currently exists no kingdom")]
    private static partial string ThereCurrentlyExistsNoKingdomTex { get; }
}
