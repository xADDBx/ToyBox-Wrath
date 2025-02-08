namespace ToyBox.Infrastructure.Localization;
public static class LocalizationManager {
    private const string DefaultLanguageCode = "en";
    private static Language? m_LocalDefault;
    private static Language? m_Local;
    private static HashSet<string>? m_FoundLanguageFiles;
    private static bool m_UsingDefaultLocale;
    private static bool IsEnabled = false;
    private static string GetPathToLocalizationFile(string LanguageCode) => Path.Combine(Main.ModEntry.Path, "Localization", LanguageCode + ".json");
    public static void Enable() {
        if (!IsEnabled) {
            try {
                Directory.CreateDirectory(Path.Combine(Main.ModEntry.Path, "Localization"));
                m_Local = null;
                m_LocalDefault = Import(DefaultLanguageCode);
                var uiLang = Settings.UILanguage;
                if (uiLang != DefaultLanguageCode) {
                    m_Local = Import(uiLang);
                }
                m_UsingDefaultLocale = m_Local == null;
                ApplyLanguage(m_UsingDefaultLocale ? m_LocalDefault : m_Local);
            } catch (Exception ex) {
                Error($"Error while trying to import configured ui language {Settings.UILanguage}\n{ex}");
            }
            IsEnabled = true;
        }
    }
    public static void Update() {
        if (!IsEnabled) {
            Enable();
        }
        var uiLang = Settings.UILanguage;
        if (uiLang == DefaultLanguageCode) {
            m_UsingDefaultLocale = true;
            m_Local = null;
        } else {
            if (!(m_Local?.LanguageCode == uiLang)) {
                try {
                    m_Local = Import(uiLang);
                    m_UsingDefaultLocale = m_Local == null;
                } catch (Exception ex) {
                    Error($"Error while trying to import configured ui language {uiLang}\n{ex}");
                }
            }
        }
        ApplyLanguage(m_UsingDefaultLocale ? m_LocalDefault : m_Local);
    }
    public static HashSet<string> GetLanguagesWithFile() {
        if (m_FoundLanguageFiles != null) return m_FoundLanguageFiles;
        m_FoundLanguageFiles = new();
        foreach (var file in Directory.GetFiles(Path.Combine(Main.ModEntry.Path, "Localization"))) {
            if (file.EndsWith(".json")) {
                
                m_FoundLanguageFiles.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
        if (m_FoundLanguageFiles.Count == 0) {
            CreateDefault();
            m_FoundLanguageFiles.Add(DefaultLanguageCode);
        }
        return m_FoundLanguageFiles;
    }
    public static Language? Import(string LanguageCode) {
        var filePath = GetPathToLocalizationFile(LanguageCode);
        if (File.Exists(filePath)) {
            return Language.Deserialize(filePath);
        } else {
            Log($"No localization file found at {filePath} for locale {LanguageCode}; Creating new.");
            return Export(LanguageCode);
        }
    }
    public static Language? Export(string languageCode) {
        try {
            m_FoundLanguageFiles = null;
            var toSerialize = languageCode == DefaultLanguageCode ? m_LocalDefault : m_Local;
            if (toSerialize == null) {
                toSerialize = CreateDefault(languageCode);
            } else {
                var dict = GatherKeys();
                foreach (var k in dict.Keys) {
                    if (!toSerialize.Strings.ContainsKey(k)) {
                        toSerialize.Strings.Add(k, dict[k]);
                    }
                }
            }
            toSerialize.LanguageCode = languageCode;
            toSerialize.Version = Main.ModEntry.Version.ToString();
            if (string.IsNullOrEmpty(toSerialize.Contributors)) toSerialize.Contributors = "The ToyBox Team";
            Language.Serialize(toSerialize, GetPathToLocalizationFile(languageCode));
            return toSerialize;
        } catch (Exception ex) {
            Error(ex);
        }
        return null;
    }
    public static void ApplyLanguage(Language? lang) {
        if (lang == null) {
            Error("Tried to apply null language!");
            return;
        }
        foreach (var pair in LocalizedStringAttribute.GetFieldsWithAttribute()) {
            if (lang.Strings.TryGetValue(pair.Item2.Key, out var str)) {
                pair.Item1.SetValue(null, str);
            } else {
                Warn($"Found no localization value for key {pair.Item2.Key} in locale {lang.LanguageCode}; Recreating keys for locale!");
                Export(lang.LanguageCode);
                Import(lang.LanguageCode);
                ApplyLanguage(m_UsingDefaultLocale ? m_LocalDefault : m_Local);
                return;
            }
        }
    }
    public static SortedDictionary<string, string> GatherKeys() {
        SortedDictionary<string, string> res = new();
        foreach (var pair in LocalizedStringAttribute.GetFieldsWithAttribute()) {
            res[pair.Item2.Key] = (pair.Item1.GetValue(null) as string)!;
        }
        return res;
    }
    public static Language CreateDefault(string languageCode = DefaultLanguageCode) {
        Language lang = new();
        lang.Strings = GatherKeys();
        lang.LanguageCode = languageCode;
        lang.Version = Main.ModEntry.Version.ToString();
        lang.Contributors = "The ToyBox Team";
        if (languageCode == DefaultLanguageCode) {
            Language.Serialize(lang, GetPathToLocalizationFile(languageCode));
        }
        return lang;
    }
}
