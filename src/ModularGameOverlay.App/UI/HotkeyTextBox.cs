using ModularGameOverlay.App.Hotkeys;
using System.ComponentModel;

namespace ModularGameOverlay.App.UI;

internal sealed class HotkeyTextBox : TextBox
{
    private HotkeyBinding _hotkey = HotkeyBinding.Empty();

    public HotkeyTextBox()
    {
        ReadOnly = true;
        ShortcutsEnabled = false;
        TabStop = true;
        TextAlign = HorizontalAlignment.Center;
        BackColor = DarkTheme.Raised;
        ForeColor = DarkTheme.Text;
        BorderStyle = BorderStyle.FixedSingle;
        Cursor = Cursors.Hand;
        MinimumSize = new Size(174, 30);
    }

    public event EventHandler? HotkeyChanged;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public HotkeyBinding Hotkey
    {
        get => _hotkey.Clone();
        set
        {
            _hotkey = value?.Clone() ?? HotkeyBinding.Empty();
            Text = _hotkey.ToDisplayString();
        }
    }

    protected override bool IsInputKey(Keys keyData) => true;

    protected override void OnEnter(EventArgs eventArgs)
    {
        base.OnEnter(eventArgs);
        BackColor = Color.FromArgb(47, 56, 70);
        SelectAll();
    }

    protected override void OnLeave(EventArgs eventArgs)
    {
        BackColor = DarkTheme.Raised;
        base.OnLeave(eventArgs);
    }

    protected override void OnKeyDown(KeyEventArgs eventArgs)
    {
        eventArgs.SuppressKeyPress = true;
        eventArgs.Handled = true;

        if (eventArgs.KeyCode is Keys.Delete or Keys.Back)
        {
            Hotkey = HotkeyBinding.Empty();
            HotkeyChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (HotkeyBinding.IsModifierKey(eventArgs.KeyCode))
        {
            Text = "Press a key with the modifiers";
            return;
        }

        var captured = HotkeyBinding.FromKeyData(eventArgs.KeyData);
        if (!captured.IsValid)
        {
            System.Media.SystemSounds.Beep.Play();
            return;
        }

        Hotkey = captured;
        HotkeyChanged?.Invoke(this, EventArgs.Empty);
    }
}
