using System.IO;
using ClipsArchiver.Entities;
using Newtonsoft.Json;

namespace ClipsArchiver.Services;

public class SettingsService
{
    private static string _settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                          @"\ClipsArchiver\settings.json";

    private static Settings? _settings;
    
    public static void SaveSettings(Settings settings)
    {
        string jsonData = JsonConvert.SerializeObject(settings);
        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                  @"\ClipsArchiver\");
        using StreamWriter createWriter = File.CreateText(_settingsPath);
        createWriter.Write(jsonData);
    }

    public static void SaveInMemorySettings(Settings settings)
    {
        _settings = settings;
    }

    public static Settings GetInMemorySetings()
    {
        return _settings ??= GetSettings();
    }
    
    public static void SaveInMemorySettingsToDisk()
    {
        if (_settings is null)
        {
            return;
        }
        SaveSettings(_settings);
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