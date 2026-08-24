using ModularGameOverlay.App.Hotkeys;

namespace ModularGameOverlay.App.Settings;

internal static class ModuleHotkeyConversions
{
    public static SuperLighter.App.HotkeyDefinition ToSuperLighter(HotkeyBinding binding) => new()
    {
        KeyCode = binding.Key,
        Control = binding.Modifiers.HasFlag(HotkeyModifiers.Control),
        Alt = binding.Modifiers.HasFlag(HotkeyModifiers.Alt),
        Shift = binding.Modifiers.HasFlag(HotkeyModifiers.Shift),
        Windows = binding.Modifiers.HasFlag(HotkeyModifiers.Windows)
    };

    public static HotkeyBinding FromSuperLighter(SuperLighter.App.HotkeyDefinition binding) => new()
    {
        Key = binding.KeyCode,
        Modifiers = FromFlags(binding.Control, binding.Alt, binding.Shift, binding.Windows)
    };

    public static Aimoro.App.HotkeyDefinition ToAimoro(HotkeyBinding binding) => new()
    {
        Key = binding.Key,
        Modifiers = (Aimoro.App.KeyModifiers)(int)binding.Modifiers
    };

    public static HotkeyBinding FromAimoro(Aimoro.App.HotkeyDefinition binding) => new()
    {
        Key = binding.Key,
        Modifiers = (HotkeyModifiers)(int)binding.Modifiers
    };

    public static SoundDirectionVisualizer.App.HotkeyDefinition ToSoundDirection(HotkeyBinding binding) => new()
    {
        Key = binding.Key,
        Modifiers = (SoundDirectionVisualizer.App.KeyModifiers)(int)binding.Modifiers
    };

    public static HotkeyBinding FromSoundDirection(SoundDirectionVisualizer.App.HotkeyDefinition binding) => new()
    {
        Key = binding.Key,
        Modifiers = (HotkeyModifiers)(int)binding.Modifiers
    };

    private static HotkeyModifiers FromFlags(bool control, bool alt, bool shift, bool windows)
    {
        var result = HotkeyModifiers.None;
        if (control) result |= HotkeyModifiers.Control;
        if (alt) result |= HotkeyModifiers.Alt;
        if (shift) result |= HotkeyModifiers.Shift;
        if (windows) result |= HotkeyModifiers.Windows;
        return result;
    }
}
