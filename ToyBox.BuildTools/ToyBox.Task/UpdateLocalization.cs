using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

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
            var options = new JsonSerializerOptions {
                WriteIndented = true,
                IncludeFields = true
            };
            string[] jsonFiles = Directory.GetFiles(LocalizationDirectoryPath, "*_lang.json");
            if (jsonFiles.Length == 0) {
                var newData = Activator.CreateInstance(languageType);
                string newFilePath = Path.Combine(LocalizationDirectoryPath, "en_lang.json");
                string jsonOutput = JsonSerializer.Serialize(newData, languageType, options);
                File.WriteAllText(newFilePath, jsonOutput);
                Log.LogMessage(MessageImportance.High, $"No JSON files found. Created a new file: {newFilePath}");
            } else {
                foreach (string file in jsonFiles) {
                    try {
                        string jsonContent = File.ReadAllText(file);
                        var data = JsonSerializer.Deserialize(jsonContent, languageType, options);
                        
                        File.WriteAllText(file, JsonSerializer.Serialize(data, languageType, options));
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