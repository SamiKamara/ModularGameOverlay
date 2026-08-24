using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aimoro.App;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly string _settingsPath;

    public SettingsStore(string? settingsPath = null)
    {
        _settingsPath = settingsPath ??
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Aimoro",
                "settings.json");
    }

    public string SettingsPath => _settingsPath;

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return CreateDefaultSettings();
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? CreateDefaultSettings();
            settings.Normalize();
            return settings;
        }
        catch
        {
            return CreateDefaultSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        settings.Normalize();
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    private static AppSettings CreateDefaultSettings()
    {
        var settings = new AppSettings();
        settings.Normalize();
        return settings;
    }
}
