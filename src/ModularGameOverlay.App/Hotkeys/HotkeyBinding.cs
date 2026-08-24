using System.Text.Json.Serialization;

namespace ModularGameOverlay.App.Hotkeys;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HotkeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Windows = 8
}

public sealed class HotkeyBinding : IEquatable<HotkeyBinding>
{
    public Keys Key { get; set; }

    public HotkeyModifiers Modifiers { get; set; }

    [JsonIgnore]
    public bool IsEmpty => Key == Keys.None;

    [JsonIgnore]
    public bool IsValid => !IsEmpty && !IsModifierKey(Key) &&
        (Modifiers != HotkeyModifiers.None || IsStandaloneKey(Key));

    public HotkeyBinding Clone() => new() { Key = Key, Modifiers = Modifiers };

    public string ToDisplayString(string emptyValue = "Not set")
    {
        if (IsEmpty)
        {
            return emptyValue;
        }

        var parts = new List<string>(5);
        if (Modifiers.HasFlag(HotkeyModifiers.Control)) parts.Add("Ctrl");
        if (Modifiers.HasFlag(HotkeyModifiers.Alt)) parts.Add("Alt");
        if (Modifiers.HasFlag(HotkeyModifiers.Shift)) parts.Add("Shift");
        if (Modifiers.HasFlag(HotkeyModifiers.Windows)) parts.Add("Win");
        parts.Add(FormatKey(Key));
        return string.Join("+", parts);
    }

    public static HotkeyBinding FromKeyData(Keys keyData)
    {
        var modifiers = HotkeyModifiers.None;
        if ((keyData & Keys.Control) == Keys.Control) modifiers |= HotkeyModifiers.Control;
        if ((keyData & Keys.Alt) == Keys.Alt) modifiers |= HotkeyModifiers.Alt;
        if ((keyData & Keys.Shift) == Keys.Shift) modifiers |= HotkeyModifiers.Shift;

        return new HotkeyBinding
        {
            Key = keyData & Keys.KeyCode,
            Modifiers = modifiers
        };
    }

    public static HotkeyBinding Empty() => new();

    public static bool IsModifierKey(Keys key) => key is
        Keys.ControlKey or Keys.LControlKey or Keys.RControlKey or
        Keys.ShiftKey or Keys.LShiftKey or Keys.RShiftKey or
        Keys.Menu or Keys.LMenu or Keys.RMenu or Keys.LWin or Keys.RWin;

    public bool Equals(HotkeyBinding? other) => other is not null &&
        Key == other.Key && Modifiers == other.Modifiers;

    public override bool Equals(object? obj) => Equals(obj as HotkeyBinding);

    public override int GetHashCode() => HashCode.Combine(Key, Modifiers);

    private static bool IsStandaloneKey(Keys key) =>
        key is >= Keys.F1 and <= Keys.F24 or Keys.Pause or Keys.PrintScreen or Keys.Scroll;

    private static string FormatKey(Keys key) => key switch
    {
        >= Keys.D0 and <= Keys.D9 => ((int)key - (int)Keys.D0).ToString(),
        Keys.Oemplus => "+",
        Keys.OemMinus => "-",
        Keys.Oemcomma => ",",
        Keys.OemPeriod => ".",
        Keys.OemQuestion => "/",
        Keys.Oemtilde => "`",
        Keys.OemOpenBrackets => "[",
        Keys.OemCloseBrackets => "]",
        Keys.OemPipe => "\\",
        Keys.OemSemicolon => ";",
        Keys.OemQuotes => "'",
        _ => key.ToString()
    };
}
