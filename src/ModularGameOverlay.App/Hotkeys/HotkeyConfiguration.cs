namespace ModularGameOverlay.App.Hotkeys;

public enum OverlayHotkeyAction
{
    ToggleLightEnhancement,
    OpenSuperLighterSettings,
    ToggleAimoroReticle,
    CycleAimoroDisplays,
    OpenAimoroSettings,
    ToggleSoundDirectionOverlay,
    CycleSoundDirectionDisplays,
    OpenSoundDirectionSettings
}

public sealed class HotkeyConfiguration
{
    public HotkeyBinding ToggleLightEnhancement { get; set; } = DefaultLightToggle();

    public HotkeyBinding OpenSuperLighterSettings { get; set; } = HotkeyBinding.Empty();

    public HotkeyBinding ToggleAimoroReticle { get; set; } = HotkeyBinding.Empty();

    public HotkeyBinding CycleAimoroDisplays { get; set; } = HotkeyBinding.Empty();

    public HotkeyBinding OpenAimoroSettings { get; set; } = HotkeyBinding.Empty();

    public HotkeyBinding ToggleSoundDirectionOverlay { get; set; } = HotkeyBinding.Empty();

    public HotkeyBinding CycleSoundDirectionDisplays { get; set; } = HotkeyBinding.Empty();

    public HotkeyBinding OpenSoundDirectionSettings { get; set; } = HotkeyBinding.Empty();

    public static HotkeyConfiguration CreateDefaults() => new();

    public HotkeyConfiguration Clone() => new()
    {
        ToggleLightEnhancement = ToggleLightEnhancement.Clone(),
        OpenSuperLighterSettings = OpenSuperLighterSettings.Clone(),
        ToggleAimoroReticle = ToggleAimoroReticle.Clone(),
        CycleAimoroDisplays = CycleAimoroDisplays.Clone(),
        OpenAimoroSettings = OpenAimoroSettings.Clone(),
        ToggleSoundDirectionOverlay = ToggleSoundDirectionOverlay.Clone(),
        CycleSoundDirectionDisplays = CycleSoundDirectionDisplays.Clone(),
        OpenSoundDirectionSettings = OpenSoundDirectionSettings.Clone()
    };

    public void Normalize()
    {
        ToggleLightEnhancement ??= DefaultLightToggle();
        OpenSuperLighterSettings ??= HotkeyBinding.Empty();
        ToggleAimoroReticle ??= HotkeyBinding.Empty();
        CycleAimoroDisplays ??= HotkeyBinding.Empty();
        OpenAimoroSettings ??= HotkeyBinding.Empty();
        ToggleSoundDirectionOverlay ??= HotkeyBinding.Empty();
        CycleSoundDirectionDisplays ??= HotkeyBinding.Empty();
        OpenSoundDirectionSettings ??= HotkeyBinding.Empty();
    }

    public IReadOnlyDictionary<OverlayHotkeyAction, HotkeyBinding> GetBindings() =>
        new Dictionary<OverlayHotkeyAction, HotkeyBinding>
        {
            [OverlayHotkeyAction.ToggleLightEnhancement] = ToggleLightEnhancement,
            [OverlayHotkeyAction.OpenSuperLighterSettings] = OpenSuperLighterSettings,
            [OverlayHotkeyAction.ToggleAimoroReticle] = ToggleAimoroReticle,
            [OverlayHotkeyAction.CycleAimoroDisplays] = CycleAimoroDisplays,
            [OverlayHotkeyAction.OpenAimoroSettings] = OpenAimoroSettings,
            [OverlayHotkeyAction.ToggleSoundDirectionOverlay] = ToggleSoundDirectionOverlay,
            [OverlayHotkeyAction.CycleSoundDirectionDisplays] = CycleSoundDirectionDisplays,
            [OverlayHotkeyAction.OpenSoundDirectionSettings] = OpenSoundDirectionSettings
        };

    public IReadOnlyList<IReadOnlyList<OverlayHotkeyAction>> FindDuplicates() =>
        GetBindings()
            .Where(entry => entry.Value.IsValid)
            .GroupBy(entry => entry.Value)
            .Where(group => group.Count() > 1)
            .Select(group => (IReadOnlyList<OverlayHotkeyAction>)group.Select(entry => entry.Key).ToArray())
            .ToArray();

    public HotkeyBinding Get(OverlayHotkeyAction action) => GetBindings()[action];

    public void Set(OverlayHotkeyAction action, HotkeyBinding binding)
    {
        var value = binding.Clone();
        switch (action)
        {
            case OverlayHotkeyAction.ToggleLightEnhancement: ToggleLightEnhancement = value; break;
            case OverlayHotkeyAction.OpenSuperLighterSettings: OpenSuperLighterSettings = value; break;
            case OverlayHotkeyAction.ToggleAimoroReticle: ToggleAimoroReticle = value; break;
            case OverlayHotkeyAction.CycleAimoroDisplays: CycleAimoroDisplays = value; break;
            case OverlayHotkeyAction.OpenAimoroSettings: OpenAimoroSettings = value; break;
            case OverlayHotkeyAction.ToggleSoundDirectionOverlay: ToggleSoundDirectionOverlay = value; break;
            case OverlayHotkeyAction.CycleSoundDirectionDisplays: CycleSoundDirectionDisplays = value; break;
            case OverlayHotkeyAction.OpenSoundDirectionSettings: OpenSoundDirectionSettings = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public static string GetLabel(OverlayHotkeyAction action) => action switch
    {
        OverlayHotkeyAction.ToggleLightEnhancement => "Toggle Light Enhancement",
        OverlayHotkeyAction.OpenSuperLighterSettings => "Open SuperLighter settings",
        OverlayHotkeyAction.ToggleAimoroReticle => "Toggle Aimoro reticle",
        OverlayHotkeyAction.CycleAimoroDisplays => "Cycle Aimoro displays",
        OverlayHotkeyAction.OpenAimoroSettings => "Open Aimoro settings",
        OverlayHotkeyAction.ToggleSoundDirectionOverlay => "Toggle sound direction overlay",
        OverlayHotkeyAction.CycleSoundDirectionDisplays => "Cycle sound direction displays",
        OverlayHotkeyAction.OpenSoundDirectionSettings => "Open sound direction settings",
        _ => action.ToString()
    };

    private static HotkeyBinding DefaultLightToggle() => new()
    {
        Key = Keys.B,
        Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt
    };
}
