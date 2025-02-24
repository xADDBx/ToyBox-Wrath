using Kingmaker;
using Newtonsoft.Json;
using System.Net;

namespace ToyBox.UpdateAndIntegrity; 
public static class VersionChecker {
    private const string LinkToIncompatibilitiesFile = "https://raw.githubusercontent.com/xADDBx/ToyBox-Wrath/main/ToyBox/Incompatibilities.json";
    // Find first entry where Mod Version of entry >= current mod version
    // Using that entry, the mod is compatible if the current game version is < than the game version in the entry
    // Meaning, an entry [x, y] will, for every mod with version <= x mark every version >= y as incompatible
    public static bool IsGameVersionSupported() {
        try {
            using var web = new WebClient();
            var raw = web.DownloadString(LinkToIncompatibilitiesFile);
            var definition = new[] { new[] { "", "" } };
            var versions = JsonConvert.DeserializeAnonymousType(raw, definition);
            var currentOrNewer = versions.FirstOrDefault(v => new Version(v[0]) >= Main.ModEntry.Version);
            if (currentOrNewer == null) return true;
            return new Version(GetNumifiedVersion(currentOrNewer[1])) > new Version(GetNumifiedVersion(GameVersion.GetVersion()));
        } catch (Exception ex) {
            Warn(ex.ToString());
        }
        return true;
    }
    public static string GetNumifiedVersion(string version) {
        var comps = version.Split('.');
        var newComps = new List<string>();
        foreach (var comp in comps) {
            uint num = 0;
            foreach (var c in comp) {
                uint newNum = num;
                try {
                    checked {
                        if (uint.TryParse(c.ToString(), out var n)) {
                            newNum = newNum * 10u + n;
                        } else {
                            int signedCharNumber = char.ToUpper(c) - ' ';
                            uint unsignedCharNumber = (uint)Math.Max(0, Math.Min(signedCharNumber, 99));
                            newNum = newNum * 100u + unsignedCharNumber;
                        }
                        num = newNum;
                    }
                } catch (OverflowException) {
                    Warn($"Encountered uint overflow while parsing version component {comp}, continuing with {num}");
                    break;
                }
            }
            newComps.Add(num.ToString());
        }
        return string.Join(".", newComps);
    }
}
