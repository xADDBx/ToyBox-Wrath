namespace ToyBox;
public abstract class FeatureWithAction : Feature {
    public void SetKeybind() {
        // TODO: GUI Work
        // Probably have a state <is setting keybind> somewhere in the GUI loop
    }
    public bool HasKeybind() {
        // TODO: IMPL
        // Maybe field?
        // Probably field.
        return false;
    }
    public class KeyBind { /* TODO: Wow */ }
    public KeyBind GetKeybind() {
        // TODO: IMPL
        return null!;
    }
    public override void Initialize() {
        base.Initialize();
        // Deserialize Keybind if exists
    }
    public abstract void ExecuteAction();
    public override void OnGui() {
        using (HorizontalScope()) {
            if (UI.Button(Name.Cyan())) {
                ExecuteAction();
            }
            Space(10);
            if (UI.Button("Bind Key".Cyan())) {
                SetKeybind();
            }
            if (HasKeybind()) {
                Space(10);
                UI.Label($"({GetKeybind()})".Cyan());
            }
            Space(10);
            UI.Label(Description.Green());
        }
    }
}
