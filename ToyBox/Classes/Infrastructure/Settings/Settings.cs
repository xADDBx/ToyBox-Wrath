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

    // BPLoader Settings
    public int BlueprintsLoaderNumShards = 32;
    public int BlueprintsLoaderChunkSize = 200;
    public int BlueprintsLoaderNumThreads = 4;
    public bool PreloadBlueprints = false;
    public bool UseBPIdCache = true;
    public bool AutomaticallyBuildBPIdCache = true;
    public bool EnableBlueprintPerformancePatches = true;
}
