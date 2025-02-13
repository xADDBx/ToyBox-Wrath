using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Text.Json;
using static ToyBox.Infrastructure.Localization;

public class UpdateLocalizationTask : Task {
    [Required]
    public string LocalizationDirectoryPath { get; set; }
    private string m_LocalizationDirectoryPath;
    public override bool Execute() {
        try {
            if (!Directory.Exists(LocalizationDirectoryPath)) {
                Log.LogMessage(MessageImportance.High, $"Directory '{LocalizationDirectoryPath}' does not exist. Creating it...");
                Directory.CreateDirectory(LocalizationDirectoryPath);
            }
            var options = new JsonSerializerOptions {
                WriteIndented = true,
                IgnoreUnknownProperties = true
            };

            string[] jsonFiles = Directory.GetFiles(LocalizationDirectoryPath, "*_lang.json");
            if (jsonFiles.Length == 0) {
                var newData = new Language();

                string newFilePath = Path.Combine(LocalizationDirectoryPath, "en_lang.json");
                string jsonOutput = JsonSerializer.Serialize(newData, options);
                File.WriteAllText(newFilePath, jsonOutput);
                Log.LogMessage(MessageImportance.High, $"No JSON files found. Created a new file: {newFilePath}");
            } else {
                foreach (string file in jsonFiles) {
                    try {
                        string jsonContent = File.ReadAllText(file);
                        var data = JsonSerializer.Deserialize<Language>(jsonContent, options);
                        
                        File.WriteAllText(file, JsonSerializer.Serialize(data, options));
                        Log.LogMessage(MessageImportance.High, $"Processed and updated file: {file}");
                    } catch (Exception ex) {
                        Log.LogMessage(MessageImportance.High, $"Error processing file {file}: {ex.Message}");
                    }
                }
            }
            return true;
        } catch (Exception ex) {
            Log.LogErrorFromException(ex, true);
            return false;
        }
    }
}