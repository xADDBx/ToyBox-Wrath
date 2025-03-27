using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
namespace ToyBox.Features.UpdateAndIntegrity; 
public static class IntegrityChecker {
    private const string ChecksumFileName = "checksum";
    public static bool CheckFilesHealthy(string? specificLocation = null) {
        var timer = Stopwatch.StartNew();
        bool isValid = true;
        try {
            string curFile;
            if (specificLocation != null) {
                curFile = Path.Combine(specificLocation, Main.ModEntry.Info.AssemblyName);
            } else {
                curFile = Assembly.GetExecutingAssembly().Location;
            }
            Log($"Checkung checksum of {curFile}");
            var curDir = Path.GetDirectoryName(curFile);
            var file = Path.Combine(curDir, ChecksumFileName);
            var providedChecksum = File.ReadAllLines(file)[0];
            using var sha256 = SHA256.Create();
            var calculatedChecksum = BitConverter.ToString(sha256.ComputeHash(File.ReadAllBytes(curFile))).Replace("-", "");
            isValid = providedChecksum.Equals(calculatedChecksum, StringComparison.OrdinalIgnoreCase);
            if (!isValid) {
                Log($"Checksum mismatch! expected: {providedChecksum}, calculated: {calculatedChecksum}");
            }
        } catch (Exception ex) {
            Warn($"Encountered exception while trying to verify checksum: {ex}");
        }
        Debug($"Finished ToyBox File Integrity Check in: {timer.ElapsedMilliseconds}ms");
        return isValid;
    }
}