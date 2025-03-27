using Newtonsoft.Json;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace ToyBox.Features.UpdateAndIntegrity; 
public static partial class Updater {
    private static bool isDoingUpdate = false;
    public static void UpdaterGUI(UnityModManager.ModEntry modEntry) {
        using (VerticalScope()) {
            using (HorizontalScope()) {
                bool pressed1 = GUILayout.Button(TryUpdatingToNewestVersionText.Cyan(), GUILayout.ExpandWidth(false));
                if (pressed1) {
                    if (!isDoingUpdate) {
                        isDoingUpdate = true;
                        Updater.Update(false, false);
                    }
                }
            }
            using (HorizontalScope()) {
                bool pressed2 = GUILayout.Button(TryReinstallCurrentVersionText.Cyan(), GUILayout.ExpandWidth(false));
                if (pressed2) {
                    if (!isDoingUpdate) {
                        isDoingUpdate = true;
                        Updater.Update(true, false);
                    }
                }
            }
        }
    }
    private static string GetReleaseName(string version) => $"ToyBox-{version}.zip";
    private static string GetDownloadLink(string repoLink, string version) => $"{repoLink}/releases/download/v{version}/{GetReleaseName(version)}";
    public static string GetLatestVersion() {
        using var web = new WebClient();
        var definition = new {
            Releases = new[] {
                    new {
                        Id = "",
                        Version = ""
                    }
            }
        };

        var raw = web.DownloadString(Main.ModEntry.Info.Repository);
        var result = JsonConvert.DeserializeAnonymousType(raw, definition);
        return result.Releases[0].Version;
    }
    public static bool Update(bool reinstallCurrentVersion = false, bool onlyUpdateIfRemoteIsNewer = true) {
        FileInfo? file = null;
        DirectoryInfo? tmpDir = null;
        bool updated = false;
        try {
            var curDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            file = new FileInfo(Path.Combine(curDir, "TmpUpdate.zip"));
            tmpDir = new DirectoryInfo(Path.Combine(curDir, "TmpExtract"));
            if (file.Exists) {
                file.Delete();
            }
            if (tmpDir.Exists) {
                tmpDir.Delete(true);
            }
            bool repoHasNewVersion = false;
            string? remoteVersion = null;
            if (!reinstallCurrentVersion) {
                remoteVersion = GetLatestVersion();
                repoHasNewVersion = new Version(VersionChecker.GetNumifiedVersion(remoteVersion)) > new Version(VersionChecker.GetNumifiedVersion(Main.ModEntry.Info.Version));
            }

            if (reinstallCurrentVersion || repoHasNewVersion || !onlyUpdateIfRemoteIsNewer) {
                var version = reinstallCurrentVersion ? Main.ModEntry.Info.Version : remoteVersion!;
                string downloadUrl = GetDownloadLink(Main.ModEntry.Info.HomePage, version);
                Log($"Downloading: {downloadUrl}");
                using var web = new WebClient();
                web.DownloadFile(downloadUrl, file.FullName);
                using var zipFile = ZipFile.OpenRead(file.FullName);

                // Dry run
                foreach (ZipArchiveEntry entry in zipFile.Entries) {
                    string fullPath = Path.GetFullPath(Path.Combine(tmpDir.FullName, entry.FullName));

                    if (Path.GetFileName(fullPath).Length == 0) {
                        Directory.CreateDirectory(fullPath);
                    } else {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                        entry.ExtractToFile(fullPath, overwrite: true);
                    }
                }

                var filesHealthy = IntegrityChecker.CheckFilesHealthy(tmpDir.FullName);
                if (filesHealthy) {
                    // Extract successfully? => Then do it again for real
                    // Note: At this point in time I only remember that I added the dry run to counter Exceptions while unpacking. I don't know why I didn't just copy the files from the dry run if it was successful.
                    // Note2: Probably because I didn't want to write a Directory Copy Helper method?
                    foreach (ZipArchiveEntry entry in zipFile.Entries) {
                        string fullPath = Path.GetFullPath(Path.Combine(curDir, entry.FullName));

                        if (Path.GetFileName(fullPath).Length == 0) {
                            Directory.CreateDirectory(fullPath);
                        } else {
                            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                            entry.ExtractToFile(fullPath, overwrite: true);
                        }
                    }

                    Log($"Successfully updated mod to version {remoteVersion}!");
                    updated = true;
                } else {
                    Warn("Extracted files failed checksum verification; aborting update.");
                }
            } else {
                Log($"Already up-to-data! Remote ({remoteVersion}) <= Local ({Main.ModEntry.Info.Version})");
            }
        } catch (Exception ex) {
            Warn($"Error trying to update mod: \n{ex.ToString()}");
        } finally {
            if (file?.Exists ?? false) {
                file.Delete();
            }
            if (tmpDir?.Exists ?? false) {
                tmpDir.Delete(true);
            }
        }
        if (updated) {
            Main.ModEntry.Info.DisplayName = "ToyBox ".Yellow().SizePercent(20) + RestartToFinishUpdateText.Green().Bold().SizePercent(40);
        }
        return updated;
    }

    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_Updater_RestartToFinishUpdateText", "Restart to finish update")]
    private static partial string RestartToFinishUpdateText { get; }
    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_Updater_TryReinstallCurrentVersionText", "Try reinstall current version")]
    private static partial string TryReinstallCurrentVersionText { get; }
    [LocalizedString("ToyBox_Features_UpdateAndIntegrity_Updater_TryUpdatingToNewestVersionText", "Try updating to newest version")]
    private static partial string TryUpdatingToNewestVersionText { get; }

}
