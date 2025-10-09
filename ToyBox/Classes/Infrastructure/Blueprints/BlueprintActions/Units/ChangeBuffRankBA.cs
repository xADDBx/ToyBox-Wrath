using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
[NeedsTesting]
public partial class ChangeBuffRankBA : BlueprintActionFeature, IBlueprintAction<BlueprintBuff>, INeedContextFeature<UnitEntityData> {

    private bool CanExecute(BlueprintBuff blueprint, out bool canDecrease, out bool canIncrease, out int rank, params object[] parameter) {
        canDecrease = false;
        canIncrease = false;
        rank = 0;
        if (parameter.Length > 0 && parameter[0] is UnitEntityData unit) {
            if (unit.GetFact(blueprint) is { } fact && blueprint.Ranks > 1) {
                rank = fact.GetRank();
                canDecrease = rank > 1;
                canIncrease = rank < blueprint.Ranks;
                return true;
            }
        }
        return false;
    }
    private bool ExecuteIncrease(BlueprintBuff blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        ((UnitEntityData)parameter[0]).GetFact<Buff>(blueprint).AddRank();
        return true;
    }
    private bool ExecuteDecrease(BlueprintBuff blueprint, params object[] parameter) {
        LogExecution(blueprint, parameter);
        ((UnitEntityData)parameter[0]).GetFact<Buff>(blueprint).RemoveRank();
        return true;
    }
    public bool? OnGui(BlueprintBuff blueprint, bool isFeatureSearch, params object[] parameter) {
        bool? result = null;
        if (CanExecute(blueprint, out var canDecrease, out var canIncrease, out var rank, parameter)) {
            if (canDecrease) {
                UI.Button(StyleActionString("<", isFeatureSearch), () => {
                    result = ExecuteDecrease(blueprint, parameter);
                });
            }
            UI.Label(StyleActionString($" {rank} ".Bold().Orange(), isFeatureSearch));
            if (canIncrease) {
                UI.Button(StyleActionString(">", isFeatureSearch), () => {
                    result = ExecuteIncrease(blueprint, parameter);
                });
            }
        } else if (isFeatureSearch) {
            UI.Label(ThisBuffHasNoRanksText.Red().Bold());
        }
        return result;
    }

    public bool GetContext(out BlueprintBuff? context) => ContextProvider.Blueprint(out context);
    public bool GetContext(out UnitEntityData? context) => ContextProvider.UnitEntityData(out context);
    public override void OnGui() {
        if (GetContext(out BlueprintBuff? bp) && GetContext(out UnitEntityData? unit)) {
            OnGui(bp!, true, unit!);
        }
    }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeBuffRankBA_Name", "Modify Buff Rank")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeBuffRankBA_Description", "Increases or decreases the value of the specified BlueprintBuff on the BlueprintUnit.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Infrastructure_Blueprints_BlueprintActions_ChangeBuffRankBA_ThisBuffHasNoRanksText", "This buff has no ranks")]
    private static partial string ThisBuffHasNoRanksText { get; }
}
