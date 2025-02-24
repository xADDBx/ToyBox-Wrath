using System.Reflection;
using System.Security.Cryptography;
namespace ToyBox.UpdateAndIntegrity; 
public static class IntegrityChecker {
    private const string ChecksumFileName = "checksum";
    public static bool CheckFilesHealthy(string? specificLocation = null) {
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
            bool isValid = providedChecksum.Equals(calculatedChecksum, StringComparison.OrdinalIgnoreCase);
            if (!isValid) {
                Log($"Checksum mismatch! expected: {providedChecksum}, calculated: {calculatedChecksum}");
            }
            return isValid;
        } catch (Exception ex) {
            Warn($"Encountered exception while trying to verify checksum: {ex.ToString()}");
            return true;
        }
    }
}