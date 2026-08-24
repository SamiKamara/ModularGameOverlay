using System.Runtime.InteropServices;

namespace ModularGameOverlay.App.UI;

internal static class DarkTheme
{
    public static readonly Color Window = Color.FromArgb(18, 21, 28);
    public static readonly Color Card = Color.FromArgb(28, 33, 43);
    public static readonly Color Raised = Color.FromArgb(39, 46, 59);
    public static readonly Color Border = Color.FromArgb(62, 72, 89);
    public static readonly Color Text = Color.FromArgb(241, 244, 249);
    public static readonly Color Muted = Color.FromArgb(165, 174, 190);
    public static readonly Color Accent = Color.FromArgb(54, 211, 183);
    public static readonly Color AccentPressed = Color.FromArgb(38, 173, 152);
    public static readonly Color Warning = Color.FromArgb(255, 187, 92);

    public static Button CreateButton(string text, bool primary = false)
    {
        var button = new Button
        {
            AutoSize = true,
            MinimumSize = new Size(126, 36),
            Padding = new Padding(12, 4, 12, 4),
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Accent : Raised,
            ForeColor = primary ? Color.FromArgb(8, 31, 29) : Text,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = primary ? Accent : Border;
        button.FlatAppearance.MouseDownBackColor = primary ? AccentPressed : Color.FromArgb(49, 57, 72);
        button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(69, 226, 198) : Color.FromArgb(46, 54, 68);
        return button;
    }

    public static CheckBox CreateToggle(string text) => new()
    {
        AutoSize = true,
        Text = text,
        ForeColor = Text,
        FlatStyle = FlatStyle.Flat,
        Cursor = Cursors.Hand,
        Padding = new Padding(2)
    };

    public static void ApplyForm(Form form)
    {
        form.BackColor = Window;
        form.ForeColor = Text;
        form.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
        form.StartPosition = FormStartPosition.CenterScreen;
        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.HandleCreated += (_, _) => EnableImmersiveDarkTitleBar(form.Handle);
    }

    private static void EnableImmersiveDarkTitleBar(IntPtr handle)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            return;
        }

        var enabled = 1;
        _ = DwmSetWindowAttribute(handle, 20, ref enabled, sizeof(int));
        _ = DwmSetWindowAttribute(handle, 19, ref enabled, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr window,
        int attribute,
        ref int value,
        int valueSize);
}
