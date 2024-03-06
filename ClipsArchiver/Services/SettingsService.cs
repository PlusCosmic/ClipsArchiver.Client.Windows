using System.IO;
using ClipsArchiver.Entities;
using Newtonsoft.Json;

namespace ClipsArchiver.Services;

public class SettingsService
{
    private static string _settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                          @"\ClipsArchiver\settings.json";
    
    public static void SaveSettings(Settings settings)
    {
        string jsonData = JsonConvert.SerializeObject(settings);
        if (!File.Exists(_settingsPath))
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                      @"\ClipsArchiver\");
            using StreamWriter createWriter = File.CreateText(_settingsPath);
            createWriter.Write(jsonData);
            return;
        }

        using FileStream fileStream = File.OpenWrite(_settingsPath);
        using StreamWriter writer = new StreamWriter(fileStream);
        writer.WriteAsync(jsonData);
    }

    public static Settings GetSettings()
    {
        if (!File.Exists(_settingsPath))
        {
           SaveSettings(Settings.GetDefaultSettings());
        }

        using StreamReader reader = File.OpenText(_settingsPath);
        string json = reader.ReadToEnd();
        Settings? settings = JsonConvert.DeserializeObject<Settings>(json);
        if (settings is null)
        {
            throw new Exception("Something went wrong retrieving settings file");
        }

        return settings;
    }
}