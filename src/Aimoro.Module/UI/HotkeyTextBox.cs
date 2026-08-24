using System.ComponentModel;

namespace Aimoro.App.UI;

public sealed class HotkeyTextBox : Control
{
    private const int BorderThickness = 1;
    private const int HorizontalTextPadding = 6;
    private HotkeyDefinition _hotkey = HotkeyDefinition.Empty();

    public HotkeyTextBox()
    {
        AccessibleRole = AccessibleRole.Text;
        BackColor = DarkUiTheme.InputBackground;
        Cursor = Cursors.IBeam;
        ForeColor = DarkUiTheme.PrimaryText;
        Size = new Size(200, 31);
        TabStop = true;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable |
            ControlStyles.UserPaint,
            true);
        Text = _hotkey.ToDisplayString();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public HotkeyDefinition Hotkey
    {
        get => _hotkey.Clone();
        set
        {
            _hotkey = value?.Clone() ?? HotkeyDefinition.Empty();
            Text = _hotkey.ToDisplayString();
        }
    }

    public event EventHandler? HotkeyChanged;

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        eventArgs.Graphics.Clear(BackColor);
        using (var border = new Pen(Focused ? DarkUiTheme.Accent : DarkUiTheme.Border))
        {
            eventArgs.Graphics.DrawRectangle(
                border,
                0,
                0,
                Math.Max(0, ClientSize.Width - 1),
                Math.Max(0, ClientSize.Height - 1));
        }

        var textBounds = new Rectangle(
            BorderThickness + HorizontalTextPadding,
            BorderThickness,
            Math.Max(0, ClientSize.Width - (2 * (BorderThickness + HorizontalTextPadding))),
            Math.Max(0, ClientSize.Height - (2 * BorderThickness)));
        TextRenderer.DrawText(
            eventArgs.Graphics,
            Text,
            Font,
            textBounds,
            Enabled ? ForeColor : DarkUiTheme.SecondaryText,
            BackColor,
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.SingleLine |
            TextFormatFlags.VerticalCenter);
    }

    protected override void OnTextChanged(EventArgs eventArgs)
    {
        base.OnTextChanged(eventArgs);
        Invalidate();
    }

    protected override void OnGotFocus(EventArgs eventArgs)
    {
        base.OnGotFocus(eventArgs);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs eventArgs)
    {
        base.OnLostFocus(eventArgs);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs eventArgs)
    {
        base.OnMouseDown(eventArgs);
        Focus();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData is Keys.Tab or (Keys.Shift | Keys.Tab))
        {
            return base.ProcessCmdKey(ref msg, keyData);
        }

        var key = keyData & Keys.KeyCode;
        if (key is Keys.Back or Keys.Delete)
        {
            Hotkey = HotkeyDefinition.Empty();
            HotkeyChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        if (HotkeyDefinition.IsModifierKey(key))
        {
            return true;
        }

        var capturedHotkey = HotkeyDefinition.FromKeyData(keyData);
        if (!capturedHotkey.IsValid)
        {
            return true;
        }

        Hotkey = capturedHotkey;
        HotkeyChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }
}
