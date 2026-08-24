using System.Text.Json;
using ModularGameOverlay.App.Settings;

namespace ModularGameOverlay.Tests;

public sealed class SettingsStoreTests
{
    [Fact]
    public void DefaultsMatchDocumentedModuleStates()
    {
        var settings = ModularGameOverlaySettings.CreateDefaults();

        Assert.False(settings.SuperLighter.Enabled);
        Assert.True(settings.Aimoro.OverlayEnabled);
        Assert.True(settings.SoundDirectionVisualizer.OverlayEnabled);
    }

    [Fact]
    public void FirstLoadMigratesModuleSettingsButUsesNewHotkeyDefaults()
    {
        using var environment = new TemporarySettingsEnvironment();
        environment.WriteLegacy("SuperLighter", """
            {
              "Enabled": false,
              "GammaPercent": 333,
              "ToggleHotkey": { "KeyCode": 70, "Control": true },
              "OpenSettingsHotkey": { "KeyCode": 79, "Alt": true }
            }
            """);
        environment.WriteLegacy("Aimoro", """
            {
              "OverlayEnabled": true,
              "ReticleLength": 37,
              "ToggleHotkey": { "Key": "A", "Modifiers": "Alt" }
            }
            """);
        environment.WriteLegacy("SoundDirectionVisualizer", """
            {
              "OverlayEnabled": true,
              "OverlayOpacityPercent": 73,
              "ToggleHotkey": { "Key": "D", "Modifiers": "Alt" }
            }
            """);

        var settings = environment.Store.Load();

        Assert.Equal(333, settings.SuperLighter.GammaPercent);
        Assert.Equal(37, settings.Aimoro.ReticleLength);
        Assert.Equal(73, settings.SoundDirectionVisualizer.OverlayOpacityPercent);
        Assert.Equal("Ctrl+Alt+B", settings.Hotkeys.ToggleLightEnhancement.ToDisplayString());
        Assert.True(settings.Hotkeys.ToggleAimoroReticle.IsEmpty);
        Assert.True(settings.Hotkeys.ToggleSoundDirectionOverlay.IsEmpty);
        Assert.True(File.Exists(environment.SettingsPath));
    }

    [Fact]
    public void ExistingUnifiedSettingsPreventAnotherLegacyMigration()
    {
        using var environment = new TemporarySettingsEnvironment();
        environment.WriteLegacy("SuperLighter", "{ \"GammaPercent\": 222 }");
        var first = environment.Store.Load();
        first.SuperLighter.GammaPercent = 444;
        environment.Store.Save(first);
        environment.WriteLegacy("SuperLighter", "{ \"GammaPercent\": 555 }");

        var second = environment.Store.Load();

        Assert.Equal(444, second.SuperLighter.GammaPercent);
    }

    [Fact]
    public void InvalidModuleSectionFallsBackWithoutResettingValidSections()
    {
        using var environment = new TemporarySettingsEnvironment();
        Directory.CreateDirectory(Path.GetDirectoryName(environment.SettingsPath)!);
        File.WriteAllText(environment.SettingsPath, """
            {
              "SuperLighter": { "Enabled": true, "GammaPercent": 321 },
              "Aimoro": "invalid module payload",
              "SoundDirectionVisualizer": { "OverlayEnabled": false, "OverlayOpacityPercent": 61 },
              "Hotkeys": {
                "ToggleLightEnhancement": { "Key": "F8", "Modifiers": "None" }
              }
            }
            """);

        var settings = environment.Store.Load();

        Assert.True(settings.SuperLighter.Enabled);
        Assert.Equal(321, settings.SuperLighter.GammaPercent);
        Assert.True(settings.Aimoro.OverlayEnabled);
        Assert.False(settings.SoundDirectionVisualizer.OverlayEnabled);
        Assert.Equal(61, settings.SoundDirectionVisualizer.OverlayOpacityPercent);
        Assert.Equal("F8", settings.Hotkeys.ToggleLightEnhancement.ToDisplayString());
    }

    private sealed class TemporarySettingsEnvironment : IDisposable
    {
        private readonly string _root = Path.Combine(
            Path.GetTempPath(),
            "ModularGameOverlay.Tests",
            Guid.NewGuid().ToString("N"));

        public TemporarySettingsEnvironment()
        {
            SettingsPath = Path.Combine(_root, "unified", "settings.json");
            Store = new SettingsStore(SettingsPath, _root);
        }

        public SettingsStore Store { get; }

        public string SettingsPath { get; }

        public void WriteLegacy(string application, string json)
        {
            var directory = Path.Combine(_root, application);
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "settings.json"), json);
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
    }
}
