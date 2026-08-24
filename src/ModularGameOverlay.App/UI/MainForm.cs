using ModularGameOverlay.App.Hotkeys;
using ModularGameOverlay.App.Settings;
using System.ComponentModel;

namespace ModularGameOverlay.App.UI;

internal sealed class MainForm : Form
{
    private const int ActionControlWidth = 174;
    private readonly CheckBox _lightEnabled = DarkTheme.CreateToggle("Enhancement enabled");
    private readonly CheckBox _aimoroEnabled = DarkTheme.CreateToggle("Reticle enabled");
    private readonly CheckBox _soundEnabled = DarkTheme.CreateToggle("Direction overlay enabled");
    private readonly HotkeyTextBox _lightHotkey = new();
    private readonly Label _status = new();
    private readonly Button _allHotkeysButton;
    private readonly List<Button> _detailedSettingsButtons = [];
    private readonly Icon _windowIcon = AppIcon.Load();
    private readonly Action<bool> _setLightEnabled;
    private readonly Action<bool> _setAimoroEnabled;
    private readonly Action<bool> _setSoundEnabled;
    private readonly Func<HotkeyBinding, string?> _setLightHotkey;
    private HotkeyBinding _committedLightHotkey = HotkeyBinding.Empty();
    private bool _updating;

    public MainForm(
        ModularGameOverlaySettings settings,
        Action<bool> setLightEnabled,
        Action<bool> setAimoroEnabled,
        Action<bool> setSoundEnabled,
        Action openSuperLighterSettings,
        Action openAimoroSettings,
        Action openSoundSettings,
        Action openHotkeys,
        Func<HotkeyBinding, string?> setLightHotkey)
    {
        _setLightEnabled = setLightEnabled;
        _setAimoroEnabled = setAimoroEnabled;
        _setSoundEnabled = setSoundEnabled;
        _setLightHotkey = setLightHotkey;

        Text = "ModularGameOverlay";
        ClientSize = new Size(720, 680);
        MinimumSize = new Size(660, 600);
        Icon = _windowIcon;
        DarkTheme.ApplyForm(this);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(24),
            BackColor = DarkTheme.Window
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var row = 0; row < root.RowCount; row++)
        {
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        Controls.Add(root);

        root.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "Modules:",
            Font = new Font("Segoe UI Semibold", 18f),
            ForeColor = DarkTheme.Text,
            Margin = new Padding(0, 0, 0, 18)
        });

        _lightEnabled.AccessibleName = "SuperLighter enhancement enabled";
        _aimoroEnabled.AccessibleName = "Aimoro reticle enabled";
        _soundEnabled.AccessibleName = "Sound direction overlay enabled";
        _lightHotkey.AccessibleName = "Toggle Light Enhancement hotkey";

        root.Controls.Add(CreateModuleCard(
            "SUPERLIGHTER",
            "Display light enhancement",
            "Gamma, contrast, saturation, overlay brightness and monitor brightness.",
            _lightEnabled,
            openSuperLighterSettings,
            "Toggle Light Enhancement hotkey",
            _lightHotkey));
        root.Controls.Add(CreateModuleCard(
            "AIMORO",
            "Reticle overlay",
            "Custom reticle, hold-to-show and automatic Steam game display targeting.",
            _aimoroEnabled,
            openAimoroSettings));
        root.Controls.Add(CreateModuleCard(
            "SOUND DIRECTION VISUALIZER",
            "Sound direction overlay",
            "Best-available stereo and multichannel direction visualization.",
            _soundEnabled,
            openSoundSettings));

        var footer = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Margin = new Padding(0, 16, 0, 0),
            Padding = new Padding(18, 0, 18, 0)
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _status.AutoSize = true;
        _status.ForeColor = DarkTheme.Muted;
        _status.Padding = new Padding(0, 10, 12, 0);
        footer.Controls.Add(_status, 0, 0);
        _allHotkeysButton = CreateActionButton("All global hotkeys...");
        _allHotkeysButton.AccessibleName = "Open all global hotkeys";
        _allHotkeysButton.Click += (_, _) => openHotkeys();
        footer.Controls.Add(_allHotkeysButton, 1, 0);
        root.Controls.Add(footer);

        _lightEnabled.CheckedChanged += (_, _) =>
        {
            if (!_updating) _setLightEnabled(_lightEnabled.Checked);
        };
        _aimoroEnabled.CheckedChanged += (_, _) =>
        {
            if (!_updating) _setAimoroEnabled(_aimoroEnabled.Checked);
        };
        _soundEnabled.CheckedChanged += (_, _) =>
        {
            if (!_updating) _setSoundEnabled(_soundEnabled.Checked);
        };
        _lightHotkey.HotkeyChanged += (_, _) =>
        {
            if (_updating)
            {
                return;
            }

            var error = _setLightHotkey(_lightHotkey.Hotkey);
            if (error is not null)
            {
                MessageBox.Show(this, error, "Invalid hotkey", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _updating = true;
                _lightHotkey.Hotkey = _committedLightHotkey;
                _updating = false;
            }
        };

        FormClosing += HandleFormClosing;
        UpdateState(settings);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AllowClose { get; set; }

    public void UpdateState(ModularGameOverlaySettings settings)
    {
        _updating = true;
        try
        {
            _lightEnabled.Checked = settings.SuperLighter.Enabled;
            _aimoroEnabled.Checked = settings.Aimoro.OverlayEnabled;
            _soundEnabled.Checked = settings.SoundDirectionVisualizer.OverlayEnabled;
            _lightHotkey.Hotkey = settings.Hotkeys.ToggleLightEnhancement;
            _committedLightHotkey = settings.Hotkeys.ToggleLightEnhancement.Clone();
            var active = new[]
            {
                settings.SuperLighter.Enabled,
                settings.Aimoro.OverlayEnabled,
                settings.SoundDirectionVisualizer.OverlayEnabled
            }.Count(enabled => enabled);
            _status.Text = $"{active} of 3 modules active · closing this window keeps the app in the tray";
        }
        finally
        {
            _updating = false;
        }
    }

    internal MainFormState GetStateForTests() => new(
        _lightEnabled.Checked,
        _aimoroEnabled.Checked,
        _soundEnabled.Checked,
        _lightHotkey.Hotkey,
        Descendants<Button>(this).Select(button => button.Text).ToArray(),
        Descendants<Control>(this).Count(control => control.Height > 0),
        Descendants<Label>(this).Select(label => label.Text).ToArray(),
        GetBoundsRelativeToForm(_lightHotkey),
        _detailedSettingsButtons.Select(GetBoundsRelativeToForm).ToArray(),
        GetBoundsRelativeToForm(_allHotkeysButton),
        _allHotkeysButton.BackColor);

    private Control CreateModuleCard(
        string eyebrow,
        string title,
        string description,
        CheckBox toggle,
        Action openSettings,
        string? extraLabel = null,
        Control? extraControl = null)
    {
        var card = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = extraControl is null ? 4 : 5,
            BackColor = DarkTheme.Card,
            Padding = new Padding(18, 14, 18, 14),
            Margin = new Padding(0, 0, 0, 12)
        };
        card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        card.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var eyebrowLabel = new Label
        {
            AutoSize = true,
            Text = eyebrow,
            ForeColor = DarkTheme.Accent,
            Font = new Font("Segoe UI Semibold", 8f)
        };
        card.Controls.Add(eyebrowLabel, 0, 0);
        var settings = CreateActionButton("Detailed settings...");
        settings.AccessibleName = $"Open {title} detailed settings";
        settings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        settings.Click += (_, _) => openSettings();
        _detailedSettingsButtons.Add(settings);
        card.Controls.Add(settings, 1, 0);
        card.Controls.Add(new Label
        {
            AutoSize = true,
            Text = title,
            Font = new Font("Segoe UI Semibold", 13f),
            ForeColor = DarkTheme.Text,
            Margin = new Padding(0, 4, 14, 0)
        }, 0, 1);
        card.Controls.Add(new Label
        {
            AutoSize = true,
            MaximumSize = new Size(450, 0),
            Text = description,
            ForeColor = DarkTheme.Muted,
            Margin = new Padding(0, 3, 14, 8)
        }, 0, 2);
        toggle.Margin = new Padding(0, 3, 14, 0);
        card.Controls.Add(toggle, 0, 3);
        if (extraControl is not null)
        {
            card.Controls.Add(new Label
            {
                AutoSize = true,
                Text = extraLabel,
                ForeColor = DarkTheme.Muted,
                Padding = new Padding(0, 7, 12, 0),
                Margin = new Padding(0, 10, 14, 0)
            }, 0, 4);
            extraControl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            extraControl.Margin = new Padding(0, 10, 0, 0);
            extraControl.Size = new Size(ActionControlWidth, 32);
            card.Controls.Add(extraControl, 1, 4);
        }
        return card;
    }

    private static Button CreateActionButton(string text)
    {
        var button = DarkTheme.CreateButton(text);
        button.AutoSize = false;
        button.Margin = Padding.Empty;
        button.Size = new Size(ActionControlWidth, 38);
        return button;
    }

    private Rectangle GetBoundsRelativeToForm(Control control)
    {
        var topLeft = control.Location;
        var parent = control.Parent;
        while (parent is not null && parent != this)
        {
            topLeft.Offset(parent.Location);
            parent = parent.Parent;
        }

        return new Rectangle(topLeft, control.Size);
    }

    private void HandleFormClosing(object? sender, FormClosingEventArgs eventArgs)
    {
        if (AllowClose || eventArgs.CloseReason == CloseReason.WindowsShutDown)
        {
            return;
        }

        eventArgs.Cancel = true;
        Hide();
    }

    private static IEnumerable<T> Descendants<T>(Control root) where T : Control
    {
        foreach (Control child in root.Controls)
        {
            if (child is T match) yield return match;
            foreach (var nested in Descendants<T>(child)) yield return nested;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _windowIcon.Dispose();
        }

        base.Dispose(disposing);
    }
}

internal sealed record MainFormState(
    bool LightEnabled,
    bool AimoroEnabled,
    bool SoundEnabled,
    HotkeyBinding LightHotkey,
    IReadOnlyList<string> ButtonLabels,
    int VisibleContentControlCount,
    IReadOnlyList<string> LabelTexts,
    Rectangle LightHotkeyBounds,
    IReadOnlyList<Rectangle> DetailedSettingsButtonBounds,
    Rectangle GlobalHotkeysButtonBounds,
    Color GlobalHotkeysButtonBackColor);
