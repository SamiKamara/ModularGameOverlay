using ModularGameOverlay.App.Hotkeys;
using System.ComponentModel;

namespace ModularGameOverlay.App.UI;

internal sealed class HotkeyTextBox : Control
{
    private const int BorderThickness = 1;
    private const int HorizontalTextPadding = 6;
    private HotkeyBinding _hotkey = HotkeyBinding.Empty();

    public HotkeyTextBox()
    {
        AccessibleRole = AccessibleRole.Text;
        BackColor = DarkTheme.Raised;
        Cursor = Cursors.Hand;
        ForeColor = DarkTheme.Text;
        MinimumSize = new Size(174, 32);
        Size = new Size(174, 32);
        TabStop = true;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.Selectable
            | ControlStyles.UserPaint,
            true);
        Text = _hotkey.ToDisplayString();
    }

    public event EventHandler? HotkeyChanged;

    internal Rectangle TextBounds
    {
        get
        {
            var textSize = TextRenderer.MeasureText(
                Text,
                Font,
                Size.Empty,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            var contentHeight = Math.Max(0, ClientSize.Height - (BorderThickness * 2));
            var textTop = BorderThickness + Math.Max(0, (contentHeight - textSize.Height) / 2);
            return new Rectangle(
                BorderThickness + HorizontalTextPadding,
                textTop,
                Math.Max(0, ClientSize.Width - (2 * (BorderThickness + HorizontalTextPadding))),
                Math.Min(textSize.Height, contentHeight));
        }
    }

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

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        eventArgs.Graphics.Clear(BackColor);
        using (var border = new Pen(Focused ? DarkTheme.Accent : DarkTheme.Border))
        {
            eventArgs.Graphics.DrawRectangle(
                border,
                0,
                0,
                Math.Max(0, ClientSize.Width - 1),
                Math.Max(0, ClientSize.Height - 1));
        }

        TextRenderer.DrawText(
            eventArgs.Graphics,
            Text,
            Font,
            TextBounds,
            Enabled ? ForeColor : DarkTheme.Muted,
            BackColor,
            TextFormatFlags.EndEllipsis
            | TextFormatFlags.HorizontalCenter
            | TextFormatFlags.NoPadding
            | TextFormatFlags.NoPrefix
            | TextFormatFlags.SingleLine
            | TextFormatFlags.VerticalCenter);
    }

    protected override void OnTextChanged(EventArgs eventArgs)
    {
        base.OnTextChanged(eventArgs);
        Invalidate();
    }

    protected override void OnEnter(EventArgs eventArgs)
    {
        base.OnEnter(eventArgs);
        BackColor = Color.FromArgb(47, 56, 70);
        Invalidate();
    }

    protected override void OnLeave(EventArgs eventArgs)
    {
        Text = _hotkey.ToDisplayString();
        BackColor = DarkTheme.Raised;
        Invalidate();
        base.OnLeave(eventArgs);
    }

    protected override void OnMouseDown(MouseEventArgs eventArgs)
    {
        base.OnMouseDown(eventArgs);
        Focus();
    }

    protected override bool ProcessCmdKey(ref Message message, Keys keyData)
    {
        if (keyData is Keys.Tab or (Keys.Shift | Keys.Tab))
        {
            return base.ProcessCmdKey(ref message, keyData);
        }

        var key = keyData & Keys.KeyCode;
        if (key is Keys.Delete or Keys.Back)
        {
            Hotkey = HotkeyBinding.Empty();
            HotkeyChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        if (HotkeyBinding.IsModifierKey(key))
        {
            Text = "Press a key with the modifiers";
            return true;
        }

        var captured = HotkeyBinding.FromKeyData(keyData);
        if (!captured.IsValid)
        {
            System.Media.SystemSounds.Beep.Play();
            return true;
        }

        Hotkey = captured;
        HotkeyChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }
}
