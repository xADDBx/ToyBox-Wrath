using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

public class UpdateLocalizationTask : Task {
    [Required]
    public string LocalizationDirectoryPath { get; set; }
    [Required]
    public string AssemblyPath { get; set; }
    public override bool Execute() {
        try {
            var asm = Assembly.Load(File.ReadAllBytes(AssemblyPath));
            var languageType = asm.GetType("ToyBox.Infrastructure.Localization.Language");
            if (languageType == null) {
                Log.LogMessage(MessageImportance.High, $"Can't find Language type in assembly at {AssemblyPath}");
                return false;
            }
            if (!Directory.Exists(LocalizationDirectoryPath)) {
                Log.LogMessage(MessageImportance.High, $"Directory '{LocalizationDirectoryPath}' does not exist. Creating it...");
                Directory.CreateDirectory(LocalizationDirectoryPath);
            }
            var options = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Populate,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Converters = [new FancyNameConverter()]
            };
            string[] jsonFiles = Directory.GetFiles(LocalizationDirectoryPath, "*_lang.json");
            var enFile = jsonFiles.FirstOrDefault(file => file.Contains("en_lang.json"));
            var enData = Activator.CreateInstance(languageType);
            if (enFile == default) {
                string newFilePath = Path.Combine(LocalizationDirectoryPath, "en_lang.json");
                string jsonOutput = JsonConvert.SerializeObject(enData, languageType, options);
                File.WriteAllText(newFilePath, jsonOutput);
                Log.LogMessage(MessageImportance.High, $"No en file found. Created a new file: {newFilePath}");
            }
            if (jsonFiles.Length > 0) {
                var fields = languageType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (string file in jsonFiles) {
                    try {
                        string jsonContent = File.ReadAllText(file);
                        var data = JsonConvert.DeserializeObject(jsonContent, languageType, options);
                        foreach (var field in fields) {
                            if (field.FieldType == typeof((string, string))) {
                                var en = ((string, string))field.GetValue(enData);
                                var other = ((string, string))field.GetValue(data);                                if (other.Item1 == other.Item2) {                                    other.Item2 = en.Item2;                                }
                                other.Item1 = en.Item1;
                                field.SetValue(data, other);
                            }
                        }

                        File.WriteAllText(file, JsonConvert.SerializeObject(data, languageType, options));
                        Log.LogMessage(MessageImportance.High, $"Processed and updated file: {file}");
                    } catch (Exception ex) {
                        Log.LogMessage(MessageImportance.High, $"Error processing file {file}: {ex.Message}");
                    }
                }
            }
        } catch (Exception ex) {
            Log.LogMessage(MessageImportance.High, ex.ToString());
            return false;
        }
        return true;
    }
}
