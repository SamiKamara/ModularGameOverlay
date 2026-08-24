using System.Text.Json;
using System.Text.Json.Serialization;
using ModularGameOverlay.App.Hotkeys;

namespace ModularGameOverlay.App.Settings;

internal sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _settingsPath;
    private readonly string _legacyRoot;

    public SettingsStore(string? settingsPath = null, string? legacyRoot = null)
    {
        var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _settingsPath = settingsPath ?? Path.Combine(applicationData, "ModularGameOverlay", "settings.json");
        _legacyRoot = legacyRoot ?? applicationData;
    }

    public string SettingsPath => _settingsPath;

    public ModularGameOverlaySettings Load()
    {
        if (File.Exists(_settingsPath))
        {
            return LoadExisting();
        }

        var migrated = MigrateLegacySettings();
        Save(migrated);
        return migrated;
    }

    public void Save(ModularGameOverlaySettings settings)
    {
        try
        {
            settings.Normalize();
            var directory = Path.GetDirectoryName(_settingsPath)!;
            Directory.CreateDirectory(directory);
            var temporaryPath = _settingsPath + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, JsonOptions));
            File.Move(temporaryPath, _settingsPath, overwrite: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private ModularGameOverlaySettings LoadExisting()
    {
        var result = ModularGameOverlaySettings.CreateDefaults();
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(_settingsPath));
            var root = document.RootElement;
            result.SchemaVersion = TryRead(root, nameof(result.SchemaVersion), result.SchemaVersion);
            result.SuperLighter = TryRead(root, nameof(result.SuperLighter), result.SuperLighter);
            result.Aimoro = TryRead(root, nameof(result.Aimoro), result.Aimoro);
            result.SoundDirectionVisualizer = TryRead(
                root,
                nameof(result.SoundDirectionVisualizer),
                result.SoundDirectionVisualizer);
            result.Hotkeys = TryRead(root, nameof(result.Hotkeys), result.Hotkeys);
        }
        catch (JsonException)
        {
            return result;
        }
        catch (IOException)
        {
            return result;
        }

        result.Normalize();
        return result;
    }

    private ModularGameOverlaySettings MigrateLegacySettings()
    {
        var result = ModularGameOverlaySettings.CreateDefaults();
        result.SuperLighter = TryReadLegacy(
            Path.Combine(_legacyRoot, "SuperLighter", "settings.json"),
            result.SuperLighter);
        result.Aimoro = TryReadLegacy(
            Path.Combine(_legacyRoot, "Aimoro", "settings.json"),
            result.Aimoro);
        result.SoundDirectionVisualizer = TryReadLegacy(
            Path.Combine(_legacyRoot, "SoundDirectionVisualizer", "settings.json"),
            result.SoundDirectionVisualizer);

        result.Hotkeys = HotkeyConfiguration.CreateDefaults();
        result.Normalize();
        return result;
    }

    private static T TryRead<T>(JsonElement root, string propertyName, T fallback)
    {
        if (!root.TryGetProperty(propertyName, out var property))
        {
            return fallback;
        }

        try
        {
            return property.Deserialize<T>(JsonOptions) ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    private static T TryReadLegacy<T>(string path, T fallback)
    {
        try
        {
            return File.Exists(path)
                ? JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions) ?? fallback
                : fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
        catch (IOException)
        {
            return fallback;
        }
    }
}
