using Kingmaker.Blueprints;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Infrastructure.Blueprints.BlueprintActions;
public interface IBlueprintAction<in T> where T : SimpleBlueprint {
    // Null - Nothing happened; False - Action execution failed; True - Action execution succeeded
    public abstract bool? OnGui(T blueprint, params object[] parameter);
}

public static class BlueprintActions {
    private static readonly Dictionary<Type, IBlueprintAction<SimpleBlueprint>> m_RegisteredTypes = [];
    private static readonly List<IBlueprintAction<SimpleBlueprint>> m_AllActions = [];
    private static readonly Dictionary<Type, IEnumerable<IBlueprintAction<SimpleBlueprint>>> m_ActionsForType = [];
    public static void LogBPAction(this IBlueprintAction<SimpleBlueprint> action, SimpleBlueprint bp, params object[] parameter) {
        Log($"Executed {action.GetType()} for blueprint {BPHelper.GetTitle(bp)} with parameter {parameter.ToContentString()}");
    }
    public static void RegisterAction(IBlueprintAction<SimpleBlueprint> action) {
        var type = action.GetType();
        if (!m_RegisteredTypes.ContainsKey(type)) {
            m_AllActions.Add(action);
            m_RegisteredTypes[type] = action;
        } else {
            Error($"Trying to register previously registered action: {type}");
        }
    }
    public static IBlueprintAction<SimpleBlueprint>? GetRegisteredAction<T>() where T : IBlueprintAction<SimpleBlueprint> {
        m_RegisteredTypes.TryGetValue(typeof(T), out var action);
        return action;
    }
    public static IEnumerable<IBlueprintAction<T>> GetActionsForBlueprintType<T>() where T : SimpleBlueprint {
        if (m_ActionsForType.TryGetValue(typeof(T), out var actions)) {
            return actions.Cast<IBlueprintAction<T>>();
        } else {
            List<IBlueprintAction<T>> newActions = [];
            foreach (var action in m_AllActions) {
                if (action is IBlueprintAction<T>) {
                    newActions.Add(action);
                }
            }
            m_ActionsForType[typeof(T)] = newActions.AsEnumerable().Cast<IBlueprintAction<SimpleBlueprint>>();
            return newActions;
        }
    }
}
