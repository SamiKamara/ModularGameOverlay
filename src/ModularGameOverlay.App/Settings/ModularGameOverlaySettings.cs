using ModularGameOverlay.App.Hotkeys;
using AimoroSettings = Aimoro.App.AppSettings;
using SoundDirectionSettings = SoundDirectionVisualizer.App.AppSettings;
using SuperLighterSettings = SuperLighter.App.AppSettings;

namespace ModularGameOverlay.App.Settings;

public sealed class ModularGameOverlaySettings
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public SuperLighterSettings SuperLighter { get; set; } = CreateSuperLighterDefaults();

    public AimoroSettings Aimoro { get; set; } = CreateAimoroDefaults();

    public SoundDirectionSettings SoundDirectionVisualizer { get; set; } = CreateSoundDirectionDefaults();

    public HotkeyConfiguration Hotkeys { get; set; } = HotkeyConfiguration.CreateDefaults();

    public static ModularGameOverlaySettings CreateDefaults()
    {
        var settings = new ModularGameOverlaySettings();
        settings.Normalize();
        return settings;
    }

    public void Normalize()
    {
        SchemaVersion = CurrentSchemaVersion;
        SuperLighter ??= CreateSuperLighterDefaults();
        Aimoro ??= CreateAimoroDefaults();
        SoundDirectionVisualizer ??= CreateSoundDirectionDefaults();
        Hotkeys ??= HotkeyConfiguration.CreateDefaults();

        SuperLighter.Normalize();
        Aimoro.Normalize();
        SoundDirectionVisualizer.Normalize();
        Hotkeys.Normalize();
        ApplyCanonicalHotkeys();
    }

    public void ApplyCanonicalHotkeys()
    {
        SuperLighter.ToggleHotkey = ModuleHotkeyConversions.ToSuperLighter(Hotkeys.ToggleLightEnhancement);
        SuperLighter.OpenSettingsHotkey = ModuleHotkeyConversions.ToSuperLighter(Hotkeys.OpenSuperLighterSettings);
        Aimoro.ToggleHotkey = ModuleHotkeyConversions.ToAimoro(Hotkeys.ToggleAimoroReticle);
        Aimoro.CycleMonitorHotkey = ModuleHotkeyConversions.ToAimoro(Hotkeys.CycleAimoroDisplays);
        Aimoro.OpenSettingsHotkey = ModuleHotkeyConversions.ToAimoro(Hotkeys.OpenAimoroSettings);
        SoundDirectionVisualizer.ToggleHotkey = ModuleHotkeyConversions.ToSoundDirection(Hotkeys.ToggleSoundDirectionOverlay);
        SoundDirectionVisualizer.CycleMonitorHotkey = ModuleHotkeyConversions.ToSoundDirection(Hotkeys.CycleSoundDirectionDisplays);
        SoundDirectionVisualizer.OpenSettingsHotkey = ModuleHotkeyConversions.ToSoundDirection(Hotkeys.OpenSoundDirectionSettings);
    }

    public void PullSuperLighterHotkeys()
    {
        Hotkeys.ToggleLightEnhancement = ModuleHotkeyConversions.FromSuperLighter(SuperLighter.ToggleHotkey);
        Hotkeys.OpenSuperLighterSettings = ModuleHotkeyConversions.FromSuperLighter(SuperLighter.OpenSettingsHotkey);
    }

    public void PullAimoroHotkeys()
    {
        Hotkeys.ToggleAimoroReticle = ModuleHotkeyConversions.FromAimoro(Aimoro.ToggleHotkey);
        Hotkeys.CycleAimoroDisplays = ModuleHotkeyConversions.FromAimoro(Aimoro.CycleMonitorHotkey);
        Hotkeys.OpenAimoroSettings = ModuleHotkeyConversions.FromAimoro(Aimoro.OpenSettingsHotkey);
    }

    public void PullSoundDirectionHotkeys()
    {
        Hotkeys.ToggleSoundDirectionOverlay = ModuleHotkeyConversions.FromSoundDirection(SoundDirectionVisualizer.ToggleHotkey);
        Hotkeys.CycleSoundDirectionDisplays = ModuleHotkeyConversions.FromSoundDirection(SoundDirectionVisualizer.CycleMonitorHotkey);
        Hotkeys.OpenSoundDirectionSettings = ModuleHotkeyConversions.FromSoundDirection(SoundDirectionVisualizer.OpenSettingsHotkey);
    }

    private static SuperLighterSettings CreateSuperLighterDefaults() => new() { Enabled = false };

    private static AimoroSettings CreateAimoroDefaults() => new() { OverlayEnabled = true };

    private static SoundDirectionSettings CreateSoundDirectionDefaults() => new() { OverlayEnabled = true };
}
