using Newtonsoft.Json;

namespace ToyBox.Infrastructure.Localization;
public class Language {
    public string LanguageCode { get; set; } = "en";
    public string Version { get; set; } = Main.ModEntry.Version.ToString();
    public string Contributors { get; set; } = "";
    public SortedDictionary<string, string> Strings { get; set; } = new();

    public static Language Deserialize(string pathToFile) {
        return JsonConvert.DeserializeObject<Language>(File.ReadAllText(pathToFile));
    }

    public static void Serialize(Language lang, string pathToFile) {
        File.WriteAllText(pathToFile, JsonConvert.SerializeObject(lang, Formatting.Indented));
    }
}