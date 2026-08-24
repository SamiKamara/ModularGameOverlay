namespace Aimoro.App;

[Flags]
public enum KeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Windows = 8
}

public sealed class HotkeyDefinition
{
    public Keys Key { get; set; }

    public KeyModifiers Modifiers { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsEmpty => Key == Keys.None;

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsValid => !IsEmpty && !IsModifierKey(Key);

    public HotkeyDefinition Clone()
    {
        return new HotkeyDefinition
        {
            Key = Key,
            Modifiers = Modifiers
        };
    }

    public string ToDisplayString(string emptyValue = "Not set")
    {
        if (IsEmpty)
        {
            return emptyValue;
        }

        var parts = new List<string>();

        if (Modifiers.HasFlag(KeyModifiers.Control))
        {
            parts.Add("Ctrl");
        }

        if (Modifiers.HasFlag(KeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (Modifiers.HasFlag(KeyModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (Modifiers.HasFlag(KeyModifiers.Windows))
        {
            parts.Add("Win");
        }

        parts.Add(Key.ToString());
        return string.Join("+", parts);
    }

    public static HotkeyDefinition FromKeyData(Keys keyData)
    {
        return new HotkeyDefinition
        {
            Key = keyData & Keys.KeyCode,
            Modifiers = GetModifiers(keyData)
        };
    }

    public static HotkeyDefinition DefaultToggle()
    {
        return new HotkeyDefinition
        {
            Key = Keys.A,
            Modifiers = KeyModifiers.Alt
        };
    }

    public static HotkeyDefinition DefaultCycle()
    {
        return new HotkeyDefinition
        {
            Key = Keys.F9,
            Modifiers = KeyModifiers.Control | KeyModifiers.Alt
        };
    }

    public static HotkeyDefinition DefaultOpenSettings()
    {
        return new HotkeyDefinition
        {
            Key = Keys.O,
            Modifiers = KeyModifiers.Alt
        };
    }

    public static HotkeyDefinition Empty()
    {
        return new HotkeyDefinition
        {
            Key = Keys.None,
            Modifiers = KeyModifiers.None
        };
    }

    public static bool IsModifierKey(Keys key)
    {
        return key is Keys.ControlKey or Keys.Menu or Keys.ShiftKey or Keys.LWin or Keys.RWin;
    }

    private static KeyModifiers GetModifiers(Keys keyData)
    {
        var modifiers = KeyModifiers.None;
        var keyModifiers = keyData & Keys.Modifiers;

        if (keyModifiers.HasFlag(Keys.Control))
        {
            modifiers |= KeyModifiers.Control;
        }

        if (keyModifiers.HasFlag(Keys.Alt))
        {
            modifiers |= KeyModifiers.Alt;
        }

        if (keyModifiers.HasFlag(Keys.Shift))
        {
            modifiers |= KeyModifiers.Shift;
        }

        return modifiers;
    }
}
