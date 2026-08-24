using Aimoro.App.Native;
using System.Drawing;

namespace Aimoro.App.UI;

public sealed class SettingsForm : Form
{
    private readonly Icon _windowIcon = LoadWindowIcon();
    private readonly Panel _scrollPanel = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        BackColor = DarkUiTheme.WindowBackground,
        Margin = Padding.Empty
    };

    private readonly TableLayoutPanel _contentPanel = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        BackColor = DarkUiTheme.WindowBackground,
        ColumnCount = 1,
        Margin = Padding.Empty
    };

    private readonly CheckBox _overlayEnabledCheckBox = DarkUiTheme.CreateCheckBox(
        "Enable the reticle when Aimoro starts");

    private readonly CheckBox _autoDetectCheckBox = DarkUiTheme.CreateCheckBox(
        "Automatically place the reticle on the monitor with a detected Steam game");

    private readonly ComboBox _monitorComboBox = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 325
    };

    private readonly HotkeyTextBox _toggleHotkeyTextBox = new() { Width = 325 };
    private readonly HotkeyTextBox _cycleHotkeyTextBox = new() { Width = 325 };
    private readonly HotkeyTextBox _openSettingsHotkeyTextBox = new() { Width = 325 };
    private readonly CheckBox _holdToShowCheckBox = DarkUiTheme.CreateCheckBox(
        "Only show the reticle while a mouse button is held");

    private readonly ComboBox _holdToShowMouseButtonComboBox = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 325
    };

    private readonly NumericUpDown _reticleLengthUpDown = CreateNumeric(4, 120);
    private readonly NumericUpDown _reticleGapUpDown = CreateNumeric(0, 60);
    private readonly NumericUpDown _reticleThicknessUpDown = CreateNumeric(1, 12);
    private readonly NumericUpDown _reticleOpacityUpDown = CreateNumeric(20, 255);
    private readonly NumericUpDown _reticleScaleUpDown = CreateScaleNumeric();
    private readonly TrackBar _reticleLengthSlider = CreateSlider(4, 50, 1, 5);
    private readonly TrackBar _reticleGapSlider = CreateSlider(0, 30, 1, 5);
    private readonly TrackBar _reticleThicknessSlider = CreateSlider(1, 8, 1, 2);
    private readonly TrackBar _reticleOpacitySlider = CreateSlider(50, 255, 5, 20);
    private readonly TrackBar _reticleScaleSlider = CreateSlider(5, 30, 1, 5);
    private readonly CheckBox _centerDotCheckBox = DarkUiTheme.CreateCheckBox("Show a center dot");

    private readonly NumericUpDown _centerDotSizeUpDown = CreateNumeric(1, 20);
    private readonly TrackBar _centerDotSizeSlider = CreateSlider(1, 10, 1, 2);
    private readonly Panel _colorPreviewPanel = CreateColorPreviewPanel();
    private readonly Panel _outlineColorPreviewPanel = CreateColorPreviewPanel();

    private readonly Button _pickColorButton = DarkUiTheme.CreateButton("Change", primary: false, 92);

    private readonly Button _pickOutlineColorButton = DarkUiTheme.CreateButton("Change", primary: false, 92);

    private readonly ColorDialog _colorDialog = new()
    {
        FullOpen = true
    };

    private Color _selectedColor;
    private Color _selectedOutlineColor;
    private bool _sizedToContent;
    private bool _bindingValues;
    private bool _syncingNumericSliders;

    public SettingsForm(AppSettings settings)
    {
        settings.Normalize();
        ResultSettings = settings.Clone();
        _selectedColor = ColorTranslator.FromHtml(settings.ReticleColorHex);
        _selectedOutlineColor = ColorTranslator.FromHtml(settings.ReticleOutlineColorHex);

        Text = "ModularGameOverlay - Aimoro";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = DarkUiTheme.WindowBackground;
        ForeColor = DarkUiTheme.PrimaryText;
        Font = new Font("Segoe UI", 9.25F, FontStyle.Regular, GraphicsUnit.Point);
        ClientSize = new Size(570, 720);
        Icon = _windowIcon;
        ShowIcon = true;

        BuildLayout();
        DarkUiTheme.ApplyTo(this);
        _bindingValues = true;
        BindValues(settings);
        _bindingValues = false;
        WireLiveApplyEvents();
    }

    public AppSettings ResultSettings { get; private set; }

    public event EventHandler? SettingsChanged;

    private static NumericUpDown CreateNumeric(int minimum, int maximum)
    {
        return new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            Width = 80
        };
    }

    private static NumericUpDown CreateScaleNumeric()
    {
        return new NumericUpDown
        {
            Minimum = 0.5m,
            Maximum = 5.0m,
            DecimalPlaces = 1,
            Increment = 0.1m,
            Width = 80
        };
    }

    private static TrackBar CreateSlider(int minimum, int maximum, int smallChange, int largeChange)
    {
        return new TrackBar
        {
            AutoSize = false,
            Height = 30,
            LargeChange = largeChange,
            Maximum = maximum,
            Minimum = minimum,
            SmallChange = smallChange,
            TickStyle = TickStyle.None,
            Width = 220
        };
    }

    private static Panel CreateColorPreviewPanel()
    {
        return new Panel
        {
            Width = 32,
            Height = 32,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 3, 0, 3)
        };
    }

    private static FlowLayoutPanel CreateNumericSliderPanel(NumericUpDown numeric, TrackBar slider)
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            BackColor = DarkUiTheme.CardBackground,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(3),
            WrapContents = false
        };

        numeric.Margin = new Padding(0, 3, 8, 3);
        slider.Margin = new Padding(0, 0, 0, 0);
        panel.Controls.Add(numeric);
        panel.Controls.Add(slider);
        return panel;
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            BackColor = DarkUiTheme.WindowBackground,
            Dock = DockStyle.Fill,
            Padding = Padding.Empty,
            ColumnCount = 1,
            RowCount = 2
        };

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var behaviorGroup = CreateBehaviorGroup();
        var hotkeysGroup = CreateHotkeysGroup();
        var reticleGroup = CreateReticleGroup();

        behaviorGroup.Margin = new Padding(0, 0, 0, 12);
        hotkeysGroup.Margin = new Padding(0, 0, 0, 12);
        reticleGroup.Margin = Padding.Empty;

        _contentPanel.Controls.Add(behaviorGroup, 0, 0);
        _contentPanel.Controls.Add(hotkeysGroup, 0, 1);
        _contentPanel.Controls.Add(reticleGroup, 0, 2);
        _contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _scrollPanel.Padding = new Padding(18, 0, 24, 18);
        _scrollPanel.Controls.Add(_contentPanel);
        root.Controls.Add(CreateHeader(), 0, 0);
        root.Controls.Add(_scrollPanel, 0, 1);

        Controls.Add(root);
    }

    private Control CreateHeader()
    {
        var header = new TableLayoutPanel
        {
            AutoSize = true,
            BackColor = DarkUiTheme.WindowBackground,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Margin = Padding.Empty,
            Padding = new Padding(22, 18, 22, 14)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var title = new Label
        {
            AutoSize = true,
            Font = new Font(Font.FontFamily, 18F, FontStyle.Bold),
            ForeColor = DarkUiTheme.PrimaryText,
            Margin = Padding.Empty,
            Text = "Aimoro"
        };
        var subtitle = new Label
        {
            AutoSize = true,
            ForeColor = DarkUiTheme.SecondaryText,
            Margin = new Padding(0, 4, 0, 0),
            MaximumSize = new Size(620, 0),
            Text = "Customize reticle appearance, display targeting, behavior, and global shortcuts."
        };

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(subtitle, 0, 1);
        return header;
    }

    private GroupBox CreateBehaviorGroup()
    {
        var group = new DarkGroupBox
        {
            Text = "Behavior",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Font = new Font(Font, FontStyle.Bold),
            Padding = new Padding(12, 10, 12, 12)
        };

        var layout = CreateTwoColumnLayout();
        layout.Font = Font;
        layout.Controls.Add(_overlayEnabledCheckBox, 0, 0);
        layout.SetColumnSpan(_overlayEnabledCheckBox, 2);

        layout.Controls.Add(_autoDetectCheckBox, 0, 1);
        layout.SetColumnSpan(_autoDetectCheckBox, 2);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Manual target monitor"
        }, 0, 2);

        layout.Controls.Add(_monitorComboBox, 1, 2);

        var helpLabel = CreateNoteLabel("If auto targeting is off, the reticle stays on the selected display.");
        layout.Controls.Add(helpLabel, 0, 3);
        layout.SetColumnSpan(helpLabel, 2);

        layout.Controls.Add(_holdToShowCheckBox, 0, 4);
        layout.SetColumnSpan(_holdToShowCheckBox, 2);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Hold button"
        }, 0, 5);

        layout.Controls.Add(_holdToShowMouseButtonComboBox, 1, 5);

        _autoDetectCheckBox.CheckedChanged += (_, _) => UpdateMonitorState();
        _holdToShowCheckBox.CheckedChanged += (_, _) => UpdateHoldModeState();

        group.Controls.Add(layout);
        return group;
    }

    private GroupBox CreateHotkeysGroup()
    {
        var group = new DarkGroupBox
        {
            Text = "Hotkeys",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Font = new Font(Font, FontStyle.Bold),
            Padding = new Padding(12, 10, 12, 12)
        };

        var layout = CreateTwoColumnLayout();
        layout.Font = Font;
        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Toggle reticle"
        }, 0, 0);
        layout.Controls.Add(_toggleHotkeyTextBox, 1, 0);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Cycle monitor"
        }, 0, 1);
        layout.Controls.Add(_cycleHotkeyTextBox, 1, 1);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Open settings"
        }, 0, 2);
        layout.Controls.Add(_openSettingsHotkeyTextBox, 1, 2);

        var noteLabel = CreateNoteLabel("Focus a hotkey box and press the combination you want. Press Delete to clear it.");
        layout.Controls.Add(noteLabel, 0, 3);
        layout.SetColumnSpan(noteLabel, 2);

        group.Controls.Add(layout);
        return group;
    }

    private GroupBox CreateReticleGroup()
    {
        var group = new DarkGroupBox
        {
            Text = "Reticle",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Font = new Font(Font, FontStyle.Bold),
            Padding = new Padding(12, 10, 12, 12)
        };

        var layout = CreateTwoColumnLayout();
        layout.Font = Font;

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Main color"
        }, 0, 0);

        var colorPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            BackColor = DarkUiTheme.CardBackground,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(3),
            WrapContents = false
        };

        colorPanel.Controls.Add(_colorPreviewPanel);
        colorPanel.Controls.Add(_pickColorButton);
        layout.Controls.Add(colorPanel, 1, 0);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Outline color"
        }, 0, 1);

        var outlineColorPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            BackColor = DarkUiTheme.CardBackground,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(3),
            WrapContents = false
        };

        outlineColorPanel.Controls.Add(_outlineColorPreviewPanel);
        outlineColorPanel.Controls.Add(_pickOutlineColorButton);
        layout.Controls.Add(outlineColorPanel, 1, 1);

        var colorSectionSpacer = new Panel
        {
            BackColor = DarkUiTheme.CardBackground,
            Height = 8,
            Margin = Padding.Empty
        };
        layout.Controls.Add(colorSectionSpacer, 0, 2);
        layout.SetColumnSpan(colorSectionSpacer, 2);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Scale (×)"
        }, 0, 3);
        layout.Controls.Add(CreateNumericSliderPanel(_reticleScaleUpDown, _reticleScaleSlider), 1, 3);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Arm length"
        }, 0, 4);
        layout.Controls.Add(CreateNumericSliderPanel(_reticleLengthUpDown, _reticleLengthSlider), 1, 4);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Gap from center"
        }, 0, 5);
        layout.Controls.Add(CreateNumericSliderPanel(_reticleGapUpDown, _reticleGapSlider), 1, 5);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Line thickness"
        }, 0, 6);
        layout.Controls.Add(CreateNumericSliderPanel(_reticleThicknessUpDown, _reticleThicknessSlider), 1, 6);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Opacity"
        }, 0, 7);
        layout.Controls.Add(CreateNumericSliderPanel(_reticleOpacityUpDown, _reticleOpacitySlider), 1, 7);

        layout.Controls.Add(_centerDotCheckBox, 0, 8);
        layout.SetColumnSpan(_centerDotCheckBox, 2);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "Center dot size"
        }, 0, 9);
        layout.Controls.Add(CreateNumericSliderPanel(_centerDotSizeUpDown, _centerDotSizeSlider), 1, 9);

        _pickColorButton.Click += (_, _) => PickColor();
        _pickOutlineColorButton.Click += (_, _) => PickOutlineColor();
        _colorPreviewPanel.Click += (_, _) => PickColor();
        _outlineColorPreviewPanel.Click += (_, _) => PickOutlineColor();
        _centerDotCheckBox.CheckedChanged += (_, _) => UpdateCenterDotSizeState();

        group.Controls.Add(layout);
        return group;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (_sizedToContent)
        {
            return;
        }

        AdjustSizeToContent();
        _sizedToContent = true;
        CenterToScreen();
    }

    protected override void OnHandleCreated(EventArgs eventArgs)
    {
        base.OnHandleCreated(eventArgs);
        var darkModeEnabled = 1;
        const int useImmersiveDarkMode = 20;
        const int useImmersiveDarkModeBefore20H1 = 19;
        if (NativeMethods.DwmSetWindowAttribute(
                Handle,
                useImmersiveDarkMode,
                ref darkModeEnabled,
                sizeof(int)) != 0)
        {
            NativeMethods.DwmSetWindowAttribute(
                Handle,
                useImmersiveDarkModeBefore20H1,
                ref darkModeEnabled,
                sizeof(int));
        }
    }

    private void AdjustSizeToContent()
    {
        var targetClientWidth = 570;

        var workingArea = Screen.FromPoint(Cursor.Position).WorkingArea;
        var nonClientWidth = Width - ClientSize.Width;
        var nonClientHeight = Height - ClientSize.Height;
        var maxClientWidth = Math.Max(550, workingArea.Width - nonClientWidth - 32);
        var maxClientHeight = Math.Max(560, workingArea.Height - nonClientHeight - 32);

        var clientWidth = Math.Min(targetClientWidth, maxClientWidth);
        ClientSize = new Size(clientWidth, ClientSize.Height);

        PerformLayout();

        var contentSize = _contentPanel.GetPreferredSize(new Size(clientWidth - _scrollPanel.Padding.Horizontal, 0));
        var desiredClientHeight = Math.Max(
            654,
            _scrollPanel.Top + contentSize.Height + _scrollPanel.Padding.Vertical);

        ClientSize = new Size(
            clientWidth,
            Math.Min(desiredClientHeight, maxClientHeight));
    }

    private static TableLayoutPanel CreateTwoColumnLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = DarkUiTheme.CardBackground,
            ColumnCount = 2,
            Padding = new Padding(10, 4, 10, 8)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        return layout;
    }

    private static Label CreateNoteLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            ForeColor = DarkUiTheme.SecondaryText,
            MaximumSize = new Size(560, 0),
            Text = text
        };
    }

    private void BindValues(AppSettings settings)
    {
        _overlayEnabledCheckBox.Checked = settings.OverlayEnabled;
        _autoDetectCheckBox.Checked = settings.AutoDetectSteamGameMonitor;

        foreach (var screen in Screen.AllScreens)
        {
            _monitorComboBox.Items.Add(new DisplayOption(screen.DeviceName, DisplayInfoFormatter.ToDisplayLabel(screen)));
        }

        foreach (var mouseButton in Enum.GetValues<HoldToShowMouseButton>())
        {
            _holdToShowMouseButtonComboBox.Items.Add(
                new HoldMouseButtonOption(mouseButton, mouseButton.ToDisplayString()));
        }

        var selectedOption = _monitorComboBox.Items
            .OfType<DisplayOption>()
            .FirstOrDefault(option => string.Equals(option.DeviceName, settings.SelectedMonitorDeviceName, StringComparison.OrdinalIgnoreCase));

        _monitorComboBox.SelectedItem = selectedOption ?? _monitorComboBox.Items.OfType<DisplayOption>().FirstOrDefault();

        _toggleHotkeyTextBox.Hotkey = settings.ToggleHotkey;
        _cycleHotkeyTextBox.Hotkey = settings.CycleMonitorHotkey;
        _openSettingsHotkeyTextBox.Hotkey = settings.OpenSettingsHotkey;
        _holdToShowCheckBox.Checked = settings.HoldToShowEnabled;
        _holdToShowMouseButtonComboBox.SelectedItem = _holdToShowMouseButtonComboBox.Items
            .OfType<HoldMouseButtonOption>()
            .FirstOrDefault(option => option.MouseButton == settings.HoldToShowMouseButton)
            ?? _holdToShowMouseButtonComboBox.Items.OfType<HoldMouseButtonOption>().FirstOrDefault();
        _reticleLengthUpDown.Value = settings.ReticleLength;
        _reticleGapUpDown.Value = settings.ReticleGap;
        _reticleThicknessUpDown.Value = settings.ReticleThickness;
        _reticleOpacityUpDown.Value = settings.ReticleOpacity;
        _reticleScaleUpDown.Value = settings.ReticleScale;
        _centerDotCheckBox.Checked = settings.ShowCenterDot;
        _centerDotSizeUpDown.Value = settings.CenterDotSize;
        SyncAllSlidersFromNumericValues();
        _colorPreviewPanel.BackColor = _selectedColor;
        _outlineColorPreviewPanel.BackColor = _selectedOutlineColor;

        UpdateMonitorState();
        UpdateHoldModeState();
        UpdateCenterDotSizeState();
    }

    private void UpdateMonitorState()
    {
        _monitorComboBox.Enabled = !_autoDetectCheckBox.Checked;
    }

    private void UpdateHoldModeState()
    {
        _holdToShowMouseButtonComboBox.Enabled = _holdToShowCheckBox.Checked;
    }

    private void UpdateCenterDotSizeState()
    {
        _centerDotSizeUpDown.Enabled = _centerDotCheckBox.Checked;
        _centerDotSizeSlider.Enabled = _centerDotCheckBox.Checked;
    }

    private void PickColor()
    {
        _colorDialog.Color = _selectedColor;
        if (_colorDialog.ShowDialog(this) == DialogResult.OK)
        {
            _selectedColor = _colorDialog.Color;
            _colorPreviewPanel.BackColor = _selectedColor;
            ApplyChanges();
        }
    }

    private void PickOutlineColor()
    {
        _colorDialog.Color = _selectedOutlineColor;
        if (_colorDialog.ShowDialog(this) == DialogResult.OK)
        {
            _selectedOutlineColor = _colorDialog.Color;
            _outlineColorPreviewPanel.BackColor = _selectedOutlineColor;
            ApplyChanges();
        }
    }

    private void WireLiveApplyEvents()
    {
        _overlayEnabledCheckBox.CheckedChanged += (_, _) => ApplyChanges();
        _autoDetectCheckBox.CheckedChanged += (_, _) => ApplyChanges();
        _monitorComboBox.SelectedIndexChanged += (_, _) => ApplyChanges();
        _toggleHotkeyTextBox.HotkeyChanged += (_, _) => ApplyChanges();
        _cycleHotkeyTextBox.HotkeyChanged += (_, _) => ApplyChanges();
        _openSettingsHotkeyTextBox.HotkeyChanged += (_, _) => ApplyChanges();
        _holdToShowCheckBox.CheckedChanged += (_, _) => ApplyChanges();
        _holdToShowMouseButtonComboBox.SelectedIndexChanged += (_, _) => ApplyChanges();
        WireNumericSlider(_reticleLengthUpDown, _reticleLengthSlider, 1);
        WireNumericSlider(_reticleGapUpDown, _reticleGapSlider, 1);
        WireNumericSlider(_reticleThicknessUpDown, _reticleThicknessSlider, 1);
        WireNumericSlider(_reticleOpacityUpDown, _reticleOpacitySlider, 1);
        WireNumericSlider(_reticleScaleUpDown, _reticleScaleSlider, 10);
        _centerDotCheckBox.CheckedChanged += (_, _) => ApplyChanges();
        WireNumericSlider(_centerDotSizeUpDown, _centerDotSizeSlider, 1);
    }

    private void WireNumericSlider(NumericUpDown numeric, TrackBar slider, int sliderUnitsPerValue)
    {
        numeric.ValueChanged += (_, _) =>
        {
            if (_syncingNumericSliders)
            {
                return;
            }

            _syncingNumericSliders = true;
            SetSliderValueClamped(numeric, slider, sliderUnitsPerValue);
            _syncingNumericSliders = false;
            ApplyChanges();
        };

        slider.ValueChanged += (_, _) =>
        {
            if (_syncingNumericSliders)
            {
                return;
            }

            _syncingNumericSliders = true;
            numeric.Value = slider.Value / (decimal)sliderUnitsPerValue;
            _syncingNumericSliders = false;
            ApplyChanges();
        };
    }

    private void SyncAllSlidersFromNumericValues()
    {
        SetSliderValueClamped(_reticleLengthUpDown, _reticleLengthSlider, 1);
        SetSliderValueClamped(_reticleGapUpDown, _reticleGapSlider, 1);
        SetSliderValueClamped(_reticleThicknessUpDown, _reticleThicknessSlider, 1);
        SetSliderValueClamped(_reticleOpacityUpDown, _reticleOpacitySlider, 1);
        SetSliderValueClamped(_reticleScaleUpDown, _reticleScaleSlider, 10);
        SetSliderValueClamped(_centerDotSizeUpDown, _centerDotSizeSlider, 1);
    }

    private static void SetSliderValueClamped(
        NumericUpDown numeric,
        TrackBar slider,
        int sliderUnitsPerValue)
    {
        var scaledValue = decimal.ToInt32(numeric.Value * sliderUnitsPerValue);
        slider.Value = Math.Clamp(scaledValue, slider.Minimum, slider.Maximum);
    }

    private void ApplyChanges()
    {
        if (_bindingValues)
        {
            return;
        }

        var updatedSettings = ReadSettings();

        // Keep the last valid bindings while an incomplete or duplicate shortcut
        // is being edited. All other controls can still apply immediately.
        if ((!_toggleHotkeyTextBox.Hotkey.IsEmpty && !_toggleHotkeyTextBox.Hotkey.IsValid) ||
            ValidateHotkeys() is not null)
        {
            updatedSettings.ToggleHotkey = ResultSettings.ToggleHotkey.Clone();
            updatedSettings.CycleMonitorHotkey = ResultSettings.CycleMonitorHotkey.Clone();
            updatedSettings.OpenSettingsHotkey = ResultSettings.OpenSettingsHotkey.Clone();
        }

        updatedSettings.Normalize();
        ResultSettings = updatedSettings;
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private AppSettings ReadSettings()
    {
        return new AppSettings
        {
            OverlayEnabled = _overlayEnabledCheckBox.Checked,
            AutoDetectSteamGameMonitor = _autoDetectCheckBox.Checked,
            SelectedMonitorDeviceName = (_monitorComboBox.SelectedItem as DisplayOption)?.DeviceName,
            ToggleHotkey = _toggleHotkeyTextBox.Hotkey,
            CycleMonitorHotkey = _cycleHotkeyTextBox.Hotkey,
            OpenSettingsHotkey = _openSettingsHotkeyTextBox.Hotkey,
            HoldToShowEnabled = _holdToShowCheckBox.Checked,
            HoldToShowMouseButton = (_holdToShowMouseButtonComboBox.SelectedItem as HoldMouseButtonOption)?.MouseButton ?? HoldToShowMouseButton.RightButton,
            ReticleColorHex = ColorTranslator.ToHtml(_selectedColor),
            ReticleOutlineColorHex = ColorTranslator.ToHtml(_selectedOutlineColor),
            ReticleLength = (int)_reticleLengthUpDown.Value,
            ReticleGap = (int)_reticleGapUpDown.Value,
            ReticleThickness = (int)_reticleThicknessUpDown.Value,
            ReticleOpacity = (int)_reticleOpacityUpDown.Value,
            ReticleScale = _reticleScaleUpDown.Value,
            ShowCenterDot = _centerDotCheckBox.Checked,
            CenterDotSize = (int)_centerDotSizeUpDown.Value
        };
    }

    private string? ValidateHotkeys()
    {
        var configuredHotkeys = new List<(string Label, HotkeyDefinition Hotkey)>
        {
            ("Toggle reticle", _toggleHotkeyTextBox.Hotkey),
            ("Cycle monitor", _cycleHotkeyTextBox.Hotkey),
            ("Open settings", _openSettingsHotkeyTextBox.Hotkey)
        };

        var duplicateGroup = configuredHotkeys
            .Where(entry => entry.Hotkey.IsValid)
            .GroupBy(entry => entry.Hotkey.ToDisplayString(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateGroup is null)
        {
            return null;
        }

        var duplicatedActions = string.Join(", ", duplicateGroup.Select(entry => entry.Label));
        return $"These actions are using the same hotkey ({duplicateGroup.Key}): {duplicatedActions}. Pick different shortcuts.";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _windowIcon.Dispose();
        }

        base.Dispose(disposing);
    }

    private static Icon LoadWindowIcon()
    {
        try
        {
            var extracted = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (extracted is not null)
            {
                return (Icon)extracted.Clone();
            }
        }
        catch
        {
        }

        return (Icon)SystemIcons.Application.Clone();
    }

    private sealed record DisplayOption(string DeviceName, string Label)
    {
        public override string ToString() => Label;
    }

    private sealed record HoldMouseButtonOption(HoldToShowMouseButton MouseButton, string Label)
    {
        public override string ToString() => Label;
    }
}
