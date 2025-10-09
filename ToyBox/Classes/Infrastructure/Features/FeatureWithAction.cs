using ToyBox.Infrastructure.Utilities;

namespace ToyBox;
public abstract class FeatureWithAction : Feature {
    public virtual void LogExecution(params object?[] parameter) {
        string toLog = "Executed action " + GetType().Name + "";
        if (parameter?.Length > 0) {
            toLog += " with parameters " + parameter.ToContentString();
        }
    }
    public abstract void ExecuteAction(params object[] parameter);
}
