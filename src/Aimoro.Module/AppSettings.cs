using System.Drawing;

namespace Aimoro.App;

public sealed class AppSettings
{
    public bool OverlayEnabled { get; set; } = true;

    public bool AutoDetectSteamGameMonitor { get; set; } = true;

    public string? SelectedMonitorDeviceName { get; set; }

    public HotkeyDefinition ToggleHotkey { get; set; } = HotkeyDefinition.DefaultToggle();

    public HotkeyDefinition CycleMonitorHotkey { get; set; } = HotkeyDefinition.DefaultCycle();

    public HotkeyDefinition OpenSettingsHotkey { get; set; } = HotkeyDefinition.DefaultOpenSettings();

    public bool HoldToShowEnabled { get; set; } = true;

    public HoldToShowMouseButton HoldToShowMouseButton { get; set; } = HoldToShowMouseButton.RightButton;

    public string ReticleColorHex { get; set; } = "#FFFFFF";

    public string ReticleOutlineColorHex { get; set; } = "#000000";

    public int ReticleOpacity { get; set; } = 220;

    public int ReticleLength { get; set; } = 20;

    public int ReticleGap { get; set; } = 8;

    public int ReticleThickness { get; set; } = 3;

    public decimal ReticleScale { get; set; } = 2.0m;

    public bool ShowCenterDot { get; set; } = true;

    public int CenterDotSize { get; set; } = 4;

    public AppSettings Clone()
    {
        return new AppSettings
        {
            OverlayEnabled = OverlayEnabled,
            AutoDetectSteamGameMonitor = AutoDetectSteamGameMonitor,
            SelectedMonitorDeviceName = SelectedMonitorDeviceName,
            ToggleHotkey = ToggleHotkey.Clone(),
            CycleMonitorHotkey = CycleMonitorHotkey.Clone(),
            OpenSettingsHotkey = OpenSettingsHotkey.Clone(),
            HoldToShowEnabled = HoldToShowEnabled,
            HoldToShowMouseButton = HoldToShowMouseButton,
            ReticleColorHex = ReticleColorHex,
            ReticleOutlineColorHex = ReticleOutlineColorHex,
            ReticleOpacity = ReticleOpacity,
            ReticleLength = ReticleLength,
            ReticleGap = ReticleGap,
            ReticleThickness = ReticleThickness,
            ReticleScale = ReticleScale,
            ShowCenterDot = ShowCenterDot,
            CenterDotSize = CenterDotSize
        };
    }

    public void Normalize()
    {
        ToggleHotkey ??= HotkeyDefinition.DefaultToggle();
        CycleMonitorHotkey ??= HotkeyDefinition.DefaultCycle();
        OpenSettingsHotkey ??= HotkeyDefinition.DefaultOpenSettings();

        if (!ToggleHotkey.IsEmpty && !ToggleHotkey.IsValid)
        {
            ToggleHotkey = HotkeyDefinition.DefaultToggle();
        }

        if (CycleMonitorHotkey.Key != Keys.None && !CycleMonitorHotkey.IsValid)
        {
            CycleMonitorHotkey = HotkeyDefinition.DefaultCycle();
        }

        if (OpenSettingsHotkey.Key != Keys.None && !OpenSettingsHotkey.IsValid)
        {
            OpenSettingsHotkey = HotkeyDefinition.DefaultOpenSettings();
        }

        ReticleLength = Math.Clamp(ReticleLength, 4, 120);
        ReticleGap = Math.Clamp(ReticleGap, 0, 60);
        ReticleThickness = Math.Clamp(ReticleThickness, 1, 12);
        ReticleScale = Math.Clamp(ReticleScale, 0.5m, 5.0m);
        ReticleOpacity = Math.Clamp(ReticleOpacity, 20, 255);
        CenterDotSize = Math.Clamp(CenterDotSize, 1, 20);
        HoldToShowMouseButton = Enum.IsDefined(HoldToShowMouseButton)
            ? HoldToShowMouseButton
            : HoldToShowMouseButton.RightButton;

        try
        {
            _ = ColorTranslator.FromHtml(ReticleColorHex);
        }
        catch
        {
            ReticleColorHex = "#FFFFFF";
        }

        try
        {
            _ = ColorTranslator.FromHtml(ReticleOutlineColorHex);
        }
        catch
        {
            ReticleOutlineColorHex = "#000000";
        }
    }

    public Color GetReticleColor()
    {
        Normalize();
        return ColorTranslator.FromHtml(ReticleColorHex);
    }

    public Color GetReticleOutlineColor()
    {
        Normalize();
        return ColorTranslator.FromHtml(ReticleOutlineColorHex);
    }
}
