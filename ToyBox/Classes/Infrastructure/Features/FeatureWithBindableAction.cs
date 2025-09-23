namespace ToyBox;
public abstract class FeatureWithBindableAction : FeatureWithAction {
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
