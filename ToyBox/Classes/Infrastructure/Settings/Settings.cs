namespace ToyBox.Infrastructure;

internal class Settings : AbstractSettings {
    private static readonly Lazy<Settings> _instance = new Lazy<Settings>(() => {
        var instance = new Settings();
        instance.Load();
        return instance;
    });
    public static Settings Instance => _instance.Value;
    protected override string Name => "Settings.json";

    public LogLevel LogLevel = LogLevel.Info;
    public string UILanguage = "en";
}
