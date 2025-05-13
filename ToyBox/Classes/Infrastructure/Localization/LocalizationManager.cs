using Newtonsoft.Json;
using System.Reflection;

namespace ToyBox.Infrastructure.Localization;
public static class LocalizationManager {
    public static Language CurrentLocalization = new();
    private static HashSet<string> m_FoundLanguageFiles = new();
    private static JsonSerializerSettings m_Settings = new() {
        Formatting = Formatting.Indented,
        DefaultValueHandling = DefaultValueHandling.Populate,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        Converters = [new FancyNameConverter()]
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
        CurrentLocalization = JsonConvert.DeserializeObject<Language>(File.ReadAllText(filePath), m_Settings) ?? new();
    }
    public static void DiscoverLocalizations() {
        m_FoundLanguageFiles = new();
        foreach (var file in Directory.GetFiles(Path.Combine(Main.ModEntry.Path, "Localization"))) {
            if (file.EndsWith(".json")) {
                m_FoundLanguageFiles.Add(Path.GetFileNameWithoutExtension(file).Replace("_lang", ""));
            }
        }
    }
    private static FieldInfo[]? m_LanguageTypeFields;
    public static void UpdateOrCreate(string languageCode) {
        try {
            var filePath = GetPathToLocalizationFile(languageCode);
            Language? lang = null;
            if (File.Exists(filePath)) {
                m_LanguageTypeFields ??= typeof(Language).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var enData = new Language();
                lang = JsonConvert.DeserializeObject<Language>(File.ReadAllText(filePath), m_Settings);
                foreach (var field in m_LanguageTypeFields) {
                    if (field.FieldType == typeof((string, string))) {
                        var en = ((string, string))field.GetValue(enData);
                        var other = ((string, string))field.GetValue(lang);                        if (other.Item1 == other.Item2) {                            other.Item2 = en.Item2;                        }
                        other.Item1 = en.Item1;
                        field.SetValue(lang, other);
                    }
                }
            }
            lang ??= new();
            if (!m_FoundLanguageFiles.Contains(Settings.UILanguage)) {
                m_FoundLanguageFiles.Add(languageCode);
            }
            File.WriteAllText(filePath, JsonConvert.SerializeObject(lang, m_Settings));
        } catch (Exception ex) {
            Error(ex);
        }
    }
}
