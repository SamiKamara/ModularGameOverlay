using ModularGameOverlay.App.Hotkeys;

namespace ModularGameOverlay.App.UI;

internal sealed class HotkeysForm : Form
{
    private readonly Dictionary<OverlayHotkeyAction, HotkeyTextBox> _fields = [];

    public HotkeysForm(HotkeyConfiguration source)
    {
        Result = source.Clone();
        Text = "ModularGameOverlay — Global hotkeys";
        ClientSize = new Size(650, 506);
        MinimumSize = new Size(590, 470);
        ShowInTaskbar = false;
        DarkTheme.ApplyForm(this);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(22),
            BackColor = DarkTheme.Window
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);

        root.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "Global hotkeys",
            Font = new Font("Segoe UI Semibold", 18f),
            ForeColor = DarkTheme.Text,
            Margin = new Padding(0, 0, 0, 16)
        });

        var table = new TableLayoutPanel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            BackColor = DarkTheme.Card,
            Padding = new Padding(16),
            ColumnCount = 2,
            RowCount = 0
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        root.Controls.Add(table, 0, 1);

        AddSection(table, "SUPERLIGHTER");
        AddBinding(table, OverlayHotkeyAction.ToggleLightEnhancement, source.ToggleLightEnhancement);
        AddBinding(table, OverlayHotkeyAction.OpenSuperLighterSettings, source.OpenSuperLighterSettings);
        AddSection(table, "AIMORO");
        AddBinding(table, OverlayHotkeyAction.ToggleAimoroReticle, source.ToggleAimoroReticle);
        AddBinding(table, OverlayHotkeyAction.CycleAimoroDisplays, source.CycleAimoroDisplays);
        AddBinding(table, OverlayHotkeyAction.OpenAimoroSettings, source.OpenAimoroSettings);
        AddSection(table, "SOUND DIRECTION VISUALIZER");
        AddBinding(table, OverlayHotkeyAction.ToggleSoundDirectionOverlay, source.ToggleSoundDirectionOverlay);
        AddBinding(table, OverlayHotkeyAction.CycleSoundDirectionDisplays, source.CycleSoundDirectionDisplays);
        AddBinding(table, OverlayHotkeyAction.OpenSoundDirectionSettings, source.OpenSoundDirectionSettings);

        var footer = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = new Padding(0, 16, 0, 0)
        };
        var save = DarkTheme.CreateButton("Save", primary: true);
        var cancel = DarkTheme.CreateButton("Cancel");
        save.Click += (_, _) => SaveAndClose();
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        footer.Controls.Add(save);
        footer.Controls.Add(cancel);
        footer.Controls.Add(new Label
        {
            AutoSize = true,
            ForeColor = DarkTheme.Muted,
            Text = "Delete or Backspace clears a binding.",
            Padding = new Padding(0, 9, 18, 0)
        });
        root.Controls.Add(footer, 0, 2);
        AcceptButton = save;
        CancelButton = cancel;
    }

    public HotkeyConfiguration Result { get; private set; }

    internal int BindingFieldCount => _fields.Count;

    private void AddSection(TableLayoutPanel table, string text)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var label = new Label
        {
            AutoSize = true,
            Text = text,
            ForeColor = DarkTheme.Accent,
            Font = new Font("Segoe UI Semibold", 8.5f),
            Margin = new Padding(0, row == 0 ? 0 : 14, 0, 6)
        };
        table.Controls.Add(label, 0, row);
        table.SetColumnSpan(label, 2);
    }

    private void AddBinding(TableLayoutPanel table, OverlayHotkeyAction action, HotkeyBinding binding)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(new Label
        {
            AutoSize = true,
            Text = HotkeyConfiguration.GetLabel(action),
            ForeColor = DarkTheme.Text,
            Padding = new Padding(0, 8, 18, 0),
            Margin = new Padding(0, 2, 0, 2)
        }, 0, row);
        var field = new HotkeyTextBox
        {
            Hotkey = binding,
            AccessibleName = HotkeyConfiguration.GetLabel(action),
            Margin = new Padding(0, 2, 0, 2)
        };
        _fields[action] = field;
        table.Controls.Add(field, 1, row);
    }

    private void SaveAndClose()
    {
        var candidate = HotkeyConfiguration.CreateDefaults();
        foreach (var (action, field) in _fields)
        {
            candidate.Set(action, field.Hotkey);
        }

        var invalid = candidate.GetBindings().FirstOrDefault(entry =>
            !entry.Value.IsEmpty && !entry.Value.IsValid);
        if (!invalid.Equals(default(KeyValuePair<OverlayHotkeyAction, HotkeyBinding>)))
        {
            ShowWarning($"{HotkeyConfiguration.GetLabel(invalid.Key)} is not a valid global hotkey.");
            return;
        }

        var duplicate = candidate.FindDuplicates().FirstOrDefault();
        if (duplicate is not null)
        {
            var names = string.Join(", ", duplicate.Select(HotkeyConfiguration.GetLabel));
            ShowWarning($"These actions use the same hotkey: {names}.");
            return;
        }

        Result = candidate;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ShowWarning(string message) => MessageBox.Show(
        this,
        message,
        "Invalid hotkey",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning);
}
