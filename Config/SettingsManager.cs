using System;
using System.IO;
using System.Text.Json;

namespace AntiDrag.Config;

public static class SettingsManager
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AntiDrag");

    private static readonly string SettingsFilePath = Path.Combine(AppDataFolder, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static Settings Load()
    {
        if (!File.Exists(SettingsFilePath))
            return new Settings();

        try
        {
            string json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<Settings>(json, JsonOptions) ?? new Settings();
        }
        catch
        {
            return new Settings();
        }
    }

    public static void Save(Settings settings)
    {
        Directory.CreateDirectory(AppDataFolder);
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsFilePath, json);
    }
}
