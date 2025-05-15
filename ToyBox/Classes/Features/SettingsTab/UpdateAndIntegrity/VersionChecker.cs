using Kingmaker;
using Newtonsoft.Json;
using System.Net;
using ToyBox.Infrastructure;

namespace ToyBox.Features.SettingsFeatures.UpdateAndIntegrity;
public static class VersionChecker {
    public static bool? ResultOfCheck = null;
    // Find first entry where Mod Version of entry >= current mod version
    // Using that entry, the mod is compatible if the current game version is < than the game version in the entry
    // Meaning, an entry [x, y] will, for every mod with version <= x mark every version >= y as incompatible
    public static void IsGameVersionSupported() {
        try {
            using var web = new WebClient();
            var raw = web.DownloadString(Constants.LinkToIncompatibilitiesFile);
            var definition = new[] { new[] { "", "" } };
            var versions = JsonConvert.DeserializeAnonymousType(raw, definition);
            var currentOrNewer = versions.FirstOrDefault(v => new Version(v[0]) >= Main.ModEntry.Version);
            if (currentOrNewer == null) {
                ResultOfCheck = true;
            } else {
                ResultOfCheck = new Version(GetNumifiedVersion(currentOrNewer[1])) > new Version(GetNumifiedVersion(GameVersion.GetVersion()));
            }
        } catch (Exception ex) {
            Warn(ex.ToString());
        }
    }
    internal static string GetNumifiedVersion(string version) {
        var comps = version.Split('.');
        var newComps = new List<string>();
        foreach (var comp in comps) {
            ulong num = 0;
            foreach (var c in comp) {
                ulong newNum = num;
                try {
                    checked {
                        if (ulong.TryParse(c.ToString(), out var n)) {
                            newNum = newNum * 10u + n;
                        } else {
                            long signedCharNumber = char.ToUpper(c) - ' ';
                            ulong unsignedCharNumber = (ulong)Math.Max(0, Math.Min(signedCharNumber, 99));
                            newNum = newNum * 100u + unsignedCharNumber;
                        }
                        num = newNum;
                    }
                } catch (OverflowException) {
                    Warn($"Encountered ulong overflow while parsing version component {comp}, continuing with {num}");
                    break;
                }
            }
            newComps.Add(num.ToString());
        }
        return string.Join(".", newComps);
    }
}
