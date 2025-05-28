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

    // - QoL
    public bool EnableModdedAchievements = true;
    public bool ToggleDisableTricksterMythicMusic = false;

    // - Cheats
    public bool DisableTraps = false;
    public bool ToggleLockJam = false;
    public bool ToggleUnlimitedModifierStacking = false;
    public bool HighlightHiddenObjects = false;
    public bool HighlightInFogOfWar = false;
    public bool HighlightHiddenTraps = false;
    public bool ToggleInfiniteAbilities = false;
    public bool ToggleInfiniteSpellCasts = false;
    public bool DisableRequireMaterialComponent = false;
    public bool DisableNegativePartyLevels = false;
    public bool DisablePartyAbilityDamage = false;
    public bool ToggleInfiniteActionsPerTurn = false;
    public bool ToggleInfiniteItemCharges = false;
    public bool ToggleInstantGlobalCrusadeSpells = false;
    public bool SpontaneousCasterCanCopyScrolls = false;
    public bool ToggleIgnoreEquipmentRestrictions = false;
    public bool ToggleDisableArmourMaxDexterity = false;
    public bool ToggleDisableArmourSpeedReduction = false;
    public bool ToggleDisableArcaneSpellFailure = false;
    public bool ToggleDisableSpellFailure = false;
    public bool ToggleIgnoreArmourChecksPenalty = false;
    public bool ToggleNoFriendlyFireAoEFeature = false;
    public bool ToggleFreeMetaMagic = false;
    public bool DisableFoW = false;
    public bool RestoreAbilitiesAfterCombat = false;
    public bool RestoreSpellsAfterCombat = false;
    public bool RestoreItemsAfterCombat = false;
    public bool RestAfterCombat = false;
    public bool ToggleInstantChangePartyMembers = false;
    public bool ToggleEquipmentNoWeight = false;
    public bool ToggleAllowItemUseFromInventoryDuringCombat = false;
    public bool ToggleIgnoreAlignmentRequirementsForAbilities = false;
    public bool ToggleIgnoreAllRequirementsForAbilities = false;
    public bool ToggleIgnorePetSizesForMounting = false;
    public bool ToggleAllowAnyUnitAsYourMount = false;
}
