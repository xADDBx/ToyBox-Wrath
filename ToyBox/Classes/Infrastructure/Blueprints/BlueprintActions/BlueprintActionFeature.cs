using Kingmaker.Blueprints;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public interface IExecutableAction<in T> where T : SimpleBlueprint {
    // Null - Nothing happened; False - Action execution failed; True - Action execution succeeded
    public abstract bool? OnGui(T blueprint, bool isFeatureSearch, params object[] parameter);
}
public interface IBlueprintAction<T> : IExecutableAction<T>, INeedContextFeature<T> where T : SimpleBlueprint { }
public abstract class BlueprintActionFeature : FeatureWithAction {
    private static readonly List<object> m_AllActions = [];
    private static readonly Dictionary<Type, object> m_ActionsForType = [];
    public override void ExecuteAction(params object[] parameter) {
        LogExecution(parameter);
    }
    protected BlueprintActionFeature() {
        m_AllActions.Add(this);
    }
    public static IEnumerable<IExecutableAction<T>> GetActionsForBlueprintType<T>() where T : SimpleBlueprint {
        if (m_ActionsForType.TryGetValue(typeof(T), out var actions)) {
            return (List<IExecutableAction<T>>)actions;
        } else {
            List<IExecutableAction<T>> newActions = [];
            foreach (var action in m_AllActions) {
                if (action is IExecutableAction<T> typedAction) {
                    newActions.Add(typedAction);
                }
            }
            m_ActionsForType[typeof(T)] = newActions;
            return newActions;
        }
    }
    protected string StyleActionString(string text, bool isFeatureSearch) {
        if (isFeatureSearch) {
            return text.Cyan().Bold().SizePercent(15);
        } else {
            return text;
        }
    }
}
public class BlueprintActions : FeatureTab {
    public override string Name => "Blueprint Actions (you should not be seeing this!)";
    public override bool IsHiddenFromUI => true;
    public BlueprintActions() {
        AddFeature(new LoadAreaPresetBA());
        AddFeature(new TeleportAreaBA());
        AddFeature(new TeleportAreaEnterPointBA());

        AddFeature(new AddGlobalMagicSpellBA());
        AddFeature(new AddKingdomBuffBA());
        AddFeature(new AddLeaderSkillBA());
        AddFeature(new RemoveGlobalMagicSpellBA());
        AddFeature(new RemoveKingdomBuffBA());
        AddFeature(new RemoveLeaderSkillBA());
        AddFeature(new SpawnArmyPresetBA());
        AddFeature(new TeleportGlobalMapBA());
        AddFeature(new TeleportGlobalMapPointBA());

        AddFeature(new AddItemBA());
        AddFeature(new RemoveItemBA());

        AddFeature(new ChangeFlagValueBA());
        AddFeature(new CompleteEtudeBA());
        AddFeature(new CompleteQuestBA());
        AddFeature(new CompleteQuestObjectiveBA());
        AddFeature(new LockFlagBA());
        AddFeature(new PlayCutsceneBA());
        AddFeature(new StartEtudeBA());
        AddFeature(new StartQuestBA());
        AddFeature(new StartQuestObjectiveBA());
        AddFeature(new UnlockFlagBA());
        AddFeature(new UnstartEtudeBA());

        AddFeature(new AddAbilityResourceBA());
        AddFeature(new AddFeatureSelectionBA());
        AddFeature(new AddParametrizedFeatureBA());
        AddFeature(new AddSpellbookBA());
        AddFeature(new AddUnitFactBA());
        AddFeature(new ChangeBuffRankBA());
        AddFeature(new ChangeFeatureRankBA());
        AddFeature(new ChangeSpellbookLevelBA());
        AddFeature(new RemoveAbilityResourceBA());
        AddFeature(new RemoveSpellbookBA());
        AddFeature(new RemoveUnitFactBA());
        AddFeature(new SpawnUnitBA());
    }
}
