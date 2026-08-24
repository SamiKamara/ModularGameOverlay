using ModularGameOverlay.App.Hotkeys;
using ModularGameOverlay.App.Settings;
using System.ComponentModel;

namespace ModularGameOverlay.App.UI;

internal sealed class MainForm : Form
{
    private readonly CheckBox _lightEnabled = DarkTheme.CreateToggle("Enhancement enabled");
    private readonly CheckBox _aimoroEnabled = DarkTheme.CreateToggle("Reticle enabled");
    private readonly CheckBox _soundEnabled = DarkTheme.CreateToggle("Direction overlay enabled");
    private readonly HotkeyTextBox _lightHotkey = new();
    private readonly Label _status = new();
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
        ClientSize = new Size(720, 650);
        MinimumSize = new Size(660, 600);
        Icon = AppIcon.Load();
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

        var titlePanel = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            Margin = new Padding(0, 0, 0, 18)
        };
        titlePanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "ModularGameOverlay",
            Font = new Font("Segoe UI Semibold", 21f),
            ForeColor = DarkTheme.Text
        });
        titlePanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "One control surface for your game overlays",
            ForeColor = DarkTheme.Muted,
            Margin = new Padding(2, 2, 0, 0)
        });
        root.Controls.Add(titlePanel);

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
            CreateLightHotkeyRow()));
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
            Margin = new Padding(0, 16, 0, 0)
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _status.AutoSize = true;
        _status.ForeColor = DarkTheme.Muted;
        _status.Padding = new Padding(0, 10, 12, 0);
        footer.Controls.Add(_status, 0, 0);
        var hotkeys = DarkTheme.CreateButton("All global hotkeys...", primary: true);
        hotkeys.AccessibleName = "Open all global hotkeys";
        hotkeys.Click += (_, _) => openHotkeys();
        footer.Controls.Add(hotkeys, 1, 0);
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
        Descendants<Control>(this).Count(control => control.Height > 0));

    private Control CreateLightHotkeyRow()
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 10, 0, 0)
        };
        panel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "Toggle Light Enhancement",
            ForeColor = DarkTheme.Muted,
            Padding = new Padding(0, 7, 12, 0)
        });
        panel.Controls.Add(_lightHotkey);
        return panel;
    }

    private static Control CreateModuleCard(
        string eyebrow,
        string title,
        string description,
        CheckBox toggle,
        Action openSettings,
        Control? extra = null)
    {
        var card = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 4,
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
        var settings = DarkTheme.CreateButton("Detailed settings...");
        settings.AccessibleName = $"Open {title} detailed settings";
        settings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        settings.Click += (_, _) => openSettings();
        card.Controls.Add(settings, 1, 0);
        card.SetRowSpan(settings, 4);
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
        if (extra is null)
        {
            card.Controls.Add(toggle, 0, 3);
        }
        else
        {
            var bottom = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Margin = Padding.Empty
            };
            bottom.Controls.Add(toggle);
            bottom.Controls.Add(extra);
            card.Controls.Add(bottom, 0, 3);
        }
        return card;
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
}

internal sealed record MainFormState(
    bool LightEnabled,
    bool AimoroEnabled,
    bool SoundEnabled,
    HotkeyBinding LightHotkey,
    IReadOnlyList<string> ButtonLabels,
    int VisibleContentControlCount);
