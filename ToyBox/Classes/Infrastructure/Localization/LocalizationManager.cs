using Newtonsoft.Json;

namespace ToyBox.Infrastructure.Localization;
public static class LocalizationManager {
    public static Language CurrentLocalization = new();
    private static HashSet<string> m_FoundLanguageFiles = new();
    private static JsonSerializerSettings m_Settings = new() {
        Formatting = Formatting.Indented,
        DefaultValueHandling = DefaultValueHandling.Populate,
        MissingMemberHandling = MissingMemberHandling.Ignore,
    };
    private static bool IsEnabled = false;
    private static string GetPathToLocalizationFile(string LanguageCode) => Path.Combine(Main.ModEntry.Path, "Localization", LanguageCode + "_lang.json");
    public static void Enable() {
        if (!IsEnabled) {
            try {
                DiscoverLocalizations();
                Update();
            } catch (Exception ex) {
                Error($"Error while trying to import configured ui language {Settings.UILanguage}\n{ex}");
            }
            IsEnabled = true;
        }
    }
    public static void Update() {
        if (!m_FoundLanguageFiles.Contains(Settings.UILanguage)) {
            UpdateOrCreate(Settings.UILanguage);
        }
        var filePath = GetPathToLocalizationFile(Settings.UILanguage);
        Language CurrentLocalization = JsonConvert.DeserializeObject<Language>(File.ReadAllText(filePath), m_Settings);
    }
    public static void DiscoverLocalizations() {
        m_FoundLanguageFiles = new();
        foreach (var file in Directory.GetFiles(Path.Combine(Main.ModEntry.Path, "Localization"))) {
            if (file.EndsWith(".json")) {
                m_FoundLanguageFiles.Add(Path.GetFileNameWithoutExtension(file).Replace("_lang", ""));
            }
        }
    }
    public static void UpdateOrCreate(string languageCode) {
        try {
            var filePath = GetPathToLocalizationFile(languageCode);
            Language lang = new();
            if (File.Exists(filePath)) {
                lang = JsonConvert.DeserializeObject<Language>(File.ReadAllText(filePath), m_Settings);
            } else {
                m_FoundLanguageFiles.Add(languageCode);
            }
            File.WriteAllText(filePath, JsonConvert.SerializeObject(lang, m_Settings));
        } catch (Exception ex) {
            Error(ex);
        }
    }
}
