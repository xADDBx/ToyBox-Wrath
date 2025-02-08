using System.Reflection;
namespace ToyBox.Infrastructure.Localization;

public static class LocalizationManager {
    private static Language? _localDefault;
    private static Language? _local;
    private static bool IsDefault;
    private static string GetPathToLocalizationFile(string LanguageCode) => Path.Combine(Main.ModEntry.Path, "Localization", LanguageCode + ".json");
    internal static bool isEnabled = false;
    public static void Enable() {
        try {
            Directory.CreateDirectory(Path.Combine(Main.ModEntry.Path, "Localization"));
            _local = null;
            _localDefault = Import("en");
            var uiLang = Settings.Instance.UILanguage;
            if (uiLang != "en") {
                _local = Import(uiLang);
            }
            IsDefault = _local == null;
            ApplyLanguage(IsDefault ? _localDefault : _local);
        } catch (Exception ex) {
            Error($"Error while trying to import configured ui language {Settings.Instance.UILanguage}\n{ex}");
        }
    }
    public static void Update() {
        var uiLang = Settings.Instance.UILanguage;
        if (uiLang == "en") {
            IsDefault = true;
            _local = null;
        } else {
            if (!(_local?.LanguageCode == uiLang)) {
                try {
                    _local = Import(uiLang);
                IsDefault = _local == null;
                } catch (Exception ex) {
                    Error($"Error while trying to import configured ui language {uiLang}\n{ex}");
                }
            }
        }
        ApplyLanguage(IsDefault ? _localDefault : _local);
    }
    private static HashSet<string>? _LanguageCache;
    public static HashSet<string> GetLanguagesWithFile() {
        if (_LanguageCache != null) return _LanguageCache;
        _LanguageCache = new();
        foreach (var file in Directory.GetFiles(Path.Combine(Main.ModEntry.Path, "Localization"))) {
            if (file.EndsWith(".json")) {
                _LanguageCache.Add(file.Split(Path.DirectorySeparatorChar).Last().Split('.')[0]);
            }
        }
        if (_LanguageCache.Count == 0) {
            CreateDefault();
            _LanguageCache.Add("en");
        }
        return _LanguageCache;
    }
    public static Language? Import(string LanguageCode) {
        var filePath = GetPathToLocalizationFile(LanguageCode);
        if (File.Exists(filePath)) {
            return Language.Deserialize(filePath);
        } else if (LanguageCode == "en") {
            Log($"No default localization file found at {filePath}; Creating new.");
            Language lang = CreateDefault();
            return lang;
        }
        return null;
    }
    public static bool Export(string LanguageCode) {
        try {
            _LanguageCache = null;
            var toSerialize = LanguageCode == "en" ? _localDefault : _local;
            if (toSerialize == null) {
                toSerialize = CreateDefault(LanguageCode);
            } else {
                if (LanguageCode != "en" && _localDefault != null) {
                    foreach (var k in _localDefault.Strings.Keys) {
                        if (!toSerialize.Strings.ContainsKey(k)) {
                            toSerialize.Strings.Add(k, _localDefault.Strings[k]);
                        }
                    }
                }
            }
            toSerialize.LanguageCode = LanguageCode;
            toSerialize.Version = Main.ModEntry.Version.ToString();
            if (string.IsNullOrEmpty(toSerialize.Contributors)) toSerialize.Contributors = "The ToyBox Team";
            Language.Serialize(toSerialize, GetPathToLocalizationFile(LanguageCode));
            return true;
        } catch (Exception ex) {
            Error(ex);
        }
        return false;
    }
    public static void ApplyLanguage(Language? lang) {
        if (lang == null) {
            Error("Tried to apply null language!");
            return;
        }
        foreach (var pair in Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            .Select(f => (f, f.GetCustomAttribute<LocalizedStringAttribute>())).Where(pair => pair.Item2 != null)) {
            pair.Item1.SetValue(null, lang.Strings[pair.Item2.Key]);
        }
    }
    public static SortedDictionary<string, string> GatherKeys() {
        SortedDictionary<string, string> res = new();
        foreach (var pair in Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            .Select(f => (f, f.GetCustomAttribute<LocalizedStringAttribute>())).Where(pair => pair.Item2 != null)) {
            res[pair.Item2.Key] = (pair.Item1.GetValue(null) as string)!;
        }
        return res;
    }
    public static Language CreateDefault(string LanguageCode = "en") {
        Language lang = new();
        lang.Strings = GatherKeys();
        lang.LanguageCode = LanguageCode;
        lang.Version = Main.ModEntry.Version.ToString();
        lang.Contributors = "The ToyBox Team";
        if (LanguageCode == "en") {
            Language.Serialize(lang, GetPathToLocalizationFile(LanguageCode));
        }
        return lang;
    }
}
