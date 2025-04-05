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

    // Settings Tab

    // - Stuff
    public bool SearchDescriptions = true;
    public LogLevel LogLevel = LogLevel.Info;
    public string UILanguage = "en";

    // - BPLoader Settings
    public int BlueprintsLoaderNumShards = 32;
    public int BlueprintsLoaderChunkSize = 200;
    public int BlueprintsLoaderNumThreads = 4;
    public bool PreloadBlueprints = false;
    public bool UseBPIdCache = true;
    public bool AutomaticallyBuildBPIdCache = true;
    public bool EnableBlueprintPerformancePatches = true;

    // - UpdateAndIntegrity
    public bool EnableVersionCompatibilityCheck = true;
    public bool EnableFileIntegrityCheck = true;

    // Bag of Tricks

    public bool EnableModdedAchievements = true;
    public bool ToggleLockJam = false;
    public bool SpontaneousCasterCanCopyScrolls = false;
    public bool DisableFoW = false;
    public bool RestoreAbilitiesAfterCombat = false;
    public bool RestoreSpellsAfterCombat = false;
    public bool RestoreItemsAfterCombat = false;
    public bool RestAfterCombat = false;
}
