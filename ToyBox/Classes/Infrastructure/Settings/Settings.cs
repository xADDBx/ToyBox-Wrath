namespace ToyBox.Infrastructure;

internal class GeneralSettings : AbstractSettings {
    private static readonly Lazy<GeneralSettings> _instance = new Lazy<GeneralSettings>(() => {
        var instance = new GeneralSettings();
        instance.Load();
        return instance;
    });
    public static GeneralSettings Settings => _instance.Value;
    protected override string Name => "Settings.json";

    public int SelectedTab = 0;

    public bool SearchDescriptions = true;
    public LogLevel LogLevel = LogLevel.Info;
    public string UILanguage = "en";
}
