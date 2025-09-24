using Kingmaker.Blueprints;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public interface IBlueprintAction { }
public interface IBlueprintAction<T> : IBlueprintAction, INeedContextFeature<T> where T : SimpleBlueprint {
    // Null - Nothing happened; False - Action execution failed; True - Action execution succeeded
    public abstract bool? OnGui(T blueprint, bool isFeatureSearch, params object[] parameter);
}
public abstract class BlueprintActionFeature : FeatureWithAction, IBlueprintAction {
    private static readonly List<IBlueprintAction> m_AllActions = [];
    private static readonly Dictionary<Type, IEnumerable<IBlueprintAction>> m_ActionsForType = [];
    public override void ExecuteAction(params object[] parameter) {
        LogExecution(parameter);
    }
    protected BlueprintActionFeature() {
        m_AllActions.Add(this);
    }
    public static IEnumerable<IBlueprintAction<T>> GetActionsForBlueprintType<T>() where T : SimpleBlueprint {
        if (m_ActionsForType.TryGetValue(typeof(T), out var actions)) {
            return actions.Cast<IBlueprintAction<T>>();
        } else {
            List<IBlueprintAction<T>> newActions = [];
            foreach (var action in m_AllActions) {
                if (action is IBlueprintAction<T> typedAction) {
                    newActions.Add(typedAction);
                }
            }
            m_ActionsForType[typeof(T)] = newActions.AsEnumerable().Cast<IBlueprintAction<SimpleBlueprint>>();
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

        AddFeature(new AddItemBA());
        AddFeature(new RemoveItemBA());

        AddFeature(new StartQuestBA());
        AddFeature(new CompleteQuestBA());
        
        AddFeature(new StartQuestObjectiveBA());
        AddFeature(new CompleteQuestObjectiveBA());
        
        AddFeature(new StartEtudeBA());
        AddFeature(new UnstartEtudeBA());
        AddFeature(new CompleteEtudeBA());
        
        AddFeature(new UnlockFlagBA());
        AddFeature(new LockFlagBA());
        AddFeature(new ChangeFlagValueBA());

        AddFeature(new PlayCutsceneBA());

        AddFeature(new AddSpellbookBA());
        AddFeature(new AddUnitFactBA());
        AddFeature(new ChangeSpellbookLevelBA());
        AddFeature(new RemoveSpellbookBA());
        AddFeature(new RemoveUnitFactBA());
        AddFeature(new SpawnUnitBA());
    }
}
