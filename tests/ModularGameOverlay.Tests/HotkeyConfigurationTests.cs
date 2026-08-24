using ModularGameOverlay.App.Hotkeys;

namespace ModularGameOverlay.Tests;

public sealed class HotkeyConfigurationTests
{
    [Fact]
    public void DefaultsBindOnlyLightEnhancement()
    {
        var configuration = HotkeyConfiguration.CreateDefaults();

        var bound = configuration.GetBindings()
            .Where(entry => !entry.Value.IsEmpty)
            .ToArray();

        var binding = Assert.Single(bound);
        Assert.Equal(OverlayHotkeyAction.ToggleLightEnhancement, binding.Key);
        Assert.Equal("Ctrl+Alt+B", binding.Value.ToDisplayString());
    }

    [Fact]
    public void DuplicateBindingsAreReportedForEveryConflictingAction()
    {
        var configuration = HotkeyConfiguration.CreateDefaults();
        configuration.ToggleAimoroReticle = configuration.ToggleLightEnhancement.Clone();

        var duplicate = Assert.Single(configuration.FindDuplicates());

        Assert.Contains(OverlayHotkeyAction.ToggleLightEnhancement, duplicate);
        Assert.Contains(OverlayHotkeyAction.ToggleAimoroReticle, duplicate);
    }

    [Fact]
    public void EmptyModuleToggleBindingsSurviveNormalization()
    {
        var aimoro = new Aimoro.App.AppSettings { ToggleHotkey = Aimoro.App.HotkeyDefinition.Empty() };
        var sound = new SoundDirectionVisualizer.App.AppSettings
        {
            ToggleHotkey = SoundDirectionVisualizer.App.HotkeyDefinition.Empty()
        };
        var super = new SuperLighter.App.AppSettings
        {
            ToggleHotkey = new SuperLighter.App.HotkeyDefinition()
        };

        aimoro.Normalize();
        sound.Normalize();
        super.Normalize();

        Assert.True(aimoro.ToggleHotkey.IsEmpty);
        Assert.True(sound.ToggleHotkey.IsEmpty);
        Assert.True(super.ToggleHotkey.IsEmpty);
    }

    [Fact]
    public void CanonicalBindingsAreMirroredToEveryModuleSettingsModel()
    {
        var settings = ModularGameOverlay.App.Settings.ModularGameOverlaySettings.CreateDefaults();
        settings.Hotkeys.ToggleAimoroReticle = new HotkeyBinding
        {
            Key = Keys.F9,
            Modifiers = HotkeyModifiers.Control
        };

        settings.ApplyCanonicalHotkeys();

        Assert.Equal(Keys.F9, settings.Aimoro.ToggleHotkey.Key);
        Assert.True(settings.Aimoro.ToggleHotkey.Modifiers.HasFlag(Aimoro.App.KeyModifiers.Control));
        Assert.Equal(Keys.None, settings.SoundDirectionVisualizer.ToggleHotkey.Key);
        Assert.Equal(Keys.None, settings.SuperLighter.OpenSettingsHotkey.KeyCode);
    }

    [Fact]
    public async Task IdleAudioCaptureCanBeStoppedWithoutOpeningADevice()
    {
        using var capture = new SoundDirectionVisualizer.App.Services.AudioCaptureService();

        await capture.StopAsync();

        Assert.Null(capture.CurrentStatus);
    }
}
