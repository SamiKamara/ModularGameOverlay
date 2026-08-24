using ModularGameOverlay.App.Hotkeys;
using ModularGameOverlay.App.Settings;
using ModularGameOverlay.App.UI;
using AimoroContext = Aimoro.App.AimoroApplicationContext;
using AimoroSettings = Aimoro.App.AppSettings;
using SoundContext = SoundDirectionVisualizer.App.SoundDirectionVisualizerApplicationContext;
using SoundSettings = SoundDirectionVisualizer.App.AppSettings;
using SuperContext = SuperLighter.App.SuperLighterApplicationContext;
using SuperSettings = SuperLighter.App.AppSettings;

namespace ModularGameOverlay.App;

internal sealed class ModularGameOverlayApplicationContext : ApplicationContext
{
    private readonly SettingsStore _settingsStore = new();
    private readonly EventWaitHandle _openMainSignal;
    private readonly RegisteredWaitHandle _openMainRegistration;
    private readonly EventWaitHandle _superSignal = new(false, EventResetMode.AutoReset);
    private readonly EventWaitHandle _aimoroSignal = new(false, EventResetMode.AutoReset);
    private readonly EventWaitHandle _soundSignal = new(false, EventResetMode.AutoReset);
    private readonly GlobalHotkeyManager _hotkeys = new();
    private readonly SuperContext _superLighter = null!;
    private readonly AimoroContext _aimoro = null!;
    private readonly SoundContext _soundDirection = null!;
    private readonly MainForm _mainForm;
    private readonly Icon _appIcon = AppIcon.Load();
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _trayMenu = new();
    private readonly ToolStripMenuItem _lightMenuItem;
    private readonly ToolStripMenuItem _aimoroMenuItem;
    private readonly ToolStripMenuItem _soundMenuItem;
    private readonly System.Windows.Forms.Timer _startupTimer = new() { Interval = 80 };
    private readonly HashSet<OverlayHotkeyAction> _reportedHotkeyFailures = [];
    private ModularGameOverlaySettings _settings;
    private bool _suspendHotkeys;
    private bool _applyingCanonicalHotkeys;
    private bool _isExiting;

    public ModularGameOverlayApplicationContext(EventWaitHandle openMainSignal)
    {
        _openMainSignal = openMainSignal;
        _settings = _settingsStore.Load();
        _settings.Normalize();

        _mainForm = new MainForm(
            _settings,
            enabled => _superLighter.SetEnabled(enabled),
            enabled => _aimoro.SetEnabled(enabled),
            enabled => _soundDirection.SetEnabled(enabled),
            () => OpenDetailedSettings(_superLighter.OpenSettings),
            () => OpenDetailedSettings(_aimoro.OpenSettings),
            () => OpenDetailedSettings(_soundDirection.OpenSettings),
            OpenCentralHotkeys,
            SetLightHotkeyFromMain);
        _ = _mainForm.Handle;

        _lightMenuItem = new ToolStripMenuItem(
            "Light Enhancement enabled",
            null,
            (_, _) => _superLighter.ToggleEnabled());
        _aimoroMenuItem = new ToolStripMenuItem(
            "Aimoro reticle enabled",
            null,
            (_, _) => _aimoro.ToggleOverlay());
        _soundMenuItem = new ToolStripMenuItem(
            "Sound direction overlay enabled",
            null,
            (_, _) => _soundDirection.ToggleOverlay());
        var detailedSettings = new ToolStripMenuItem("Detailed settings");
        detailedSettings.DropDownItems.Add("SuperLighter...", null, (_, _) => OpenDetailedSettings(_superLighter.OpenSettings));
        detailedSettings.DropDownItems.Add("Aimoro...", null, (_, _) => OpenDetailedSettings(_aimoro.OpenSettings));
        detailedSettings.DropDownItems.Add("Sound Direction Visualizer...", null, (_, _) => OpenDetailedSettings(_soundDirection.OpenSettings));
        var openMain = new ToolStripMenuItem("Open control panel", null, (_, _) => ShowMainWindow());
        var hotkeySettings = new ToolStripMenuItem("Global hotkeys...", null, (_, _) => OpenCentralHotkeys());
        var exit = new ToolStripMenuItem("Exit", null, (_, _) => ExitApplication());
        _trayMenu.Items.AddRange(
            openMain,
            new ToolStripSeparator(),
            _lightMenuItem,
            _aimoroMenuItem,
            _soundMenuItem,
            new ToolStripSeparator(),
            detailedSettings,
            hotkeySettings,
            new ToolStripSeparator(),
            exit);
        ConfigureTrayMenu(_trayMenu);

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = _trayMenu,
            Icon = _appIcon,
            Text = "ModularGameOverlay",
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();

        _openMainRegistration = ThreadPool.RegisterWaitForSingleObject(
            _openMainSignal,
            static (state, _) => ((ModularGameOverlayApplicationContext)state!).RequestShowMainWindow(),
            this,
            Timeout.Infinite,
            executeOnlyOnce: false);

        _hotkeys.HotkeyPressed += HandleHotkeyPressed;

        _superLighter = new SuperContext(
            _superSignal,
            _settings.SuperLighter,
            HandleSuperLighterSaved,
            embeddedHost: true);
        _aimoro = new AimoroContext(
            _aimoroSignal,
            openSettingsOnStartup: false,
            _settings.Aimoro,
            HandleAimoroSaved,
            embeddedHost: true);
        _soundDirection = new SoundContext(
            _soundSignal,
            openSettingsOnStartup: false,
            _settings.SoundDirectionVisualizer,
            HandleSoundDirectionSaved,
            embeddedHost: true);

        RegisterHotkeys();
        UpdateUiState();
        _startupTimer.Tick += HandleStartupTimer;
        _startupTimer.Start();
    }

    public void RestoreDisplayEffects() => _superLighter.RestoreDisplayEffects();

    private void HandleStartupTimer(object? sender, EventArgs eventArgs)
    {
        _startupTimer.Stop();
        ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        if (_isExiting)
        {
            return;
        }

        RegisterHotkeys();

        if (_mainForm.WindowState == FormWindowState.Minimized)
        {
            _mainForm.WindowState = FormWindowState.Normal;
        }

        _mainForm.Show();
        _mainForm.BringToFront();
        _mainForm.Activate();
    }

    private void RequestShowMainWindow()
    {
        try
        {
            if (!_mainForm.IsDisposed)
            {
                _mainForm.BeginInvoke(new MethodInvoker(ShowMainWindow));
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void OpenDetailedSettings(Action openSettings)
    {
        if (_isExiting)
        {
            return;
        }

        _suspendHotkeys = true;
        _hotkeys.ClearBindings();
        try
        {
            openSettings();
        }
        finally
        {
            _suspendHotkeys = false;
            RegisterHotkeys();
            UpdateUiState();
        }
    }

    private void OpenCentralHotkeys()
    {
        if (_isExiting)
        {
            return;
        }

        _suspendHotkeys = true;
        _hotkeys.ClearBindings();
        try
        {
            using var form = new HotkeysForm(_settings.Hotkeys);
            if (form.ShowDialog(_mainForm) == DialogResult.OK)
            {
                ApplyHotkeyConfiguration(form.Result);
            }
        }
        finally
        {
            _suspendHotkeys = false;
            RegisterHotkeys();
            UpdateUiState();
        }
    }

    private string? SetLightHotkeyFromMain(HotkeyBinding binding)
    {
        var candidate = _settings.Hotkeys.Clone();
        candidate.ToggleLightEnhancement = binding.Clone();
        var error = ValidateHotkeys(candidate);
        if (error is not null)
        {
            return error;
        }

        ApplyHotkeyConfiguration(candidate);
        return null;
    }

    private void ApplyHotkeyConfiguration(HotkeyConfiguration configuration)
    {
        var candidate = configuration.Clone();
        candidate.Normalize();
        var validationError = ValidateHotkeys(candidate);
        if (validationError is not null)
        {
            MessageBox.Show(validationError, "Invalid hotkey", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _settings.Hotkeys = candidate;
        _settings.ApplyCanonicalHotkeys();

        _applyingCanonicalHotkeys = true;
        try
        {
            _superLighter.SetHotkeys(
                ModuleHotkeyConversions.ToSuperLighter(candidate.ToggleLightEnhancement),
                ModuleHotkeyConversions.ToSuperLighter(candidate.OpenSuperLighterSettings));
            _aimoro.SetHotkeys(
                ModuleHotkeyConversions.ToAimoro(candidate.ToggleAimoroReticle),
                ModuleHotkeyConversions.ToAimoro(candidate.CycleAimoroDisplays),
                ModuleHotkeyConversions.ToAimoro(candidate.OpenAimoroSettings));
            _soundDirection.SetHotkeys(
                ModuleHotkeyConversions.ToSoundDirection(candidate.ToggleSoundDirectionOverlay),
                ModuleHotkeyConversions.ToSoundDirection(candidate.CycleSoundDirectionDisplays),
                ModuleHotkeyConversions.ToSoundDirection(candidate.OpenSoundDirectionSettings));
        }
        finally
        {
            _applyingCanonicalHotkeys = false;
        }

        SaveSettings();
        RegisterHotkeys();
        UpdateUiState();
    }

    private static string? ValidateHotkeys(HotkeyConfiguration candidate)
    {
        var invalid = candidate.GetBindings().FirstOrDefault(entry =>
            !entry.Value.IsEmpty && !entry.Value.IsValid);
        if (invalid.Value is not null)
        {
            return $"{HotkeyConfiguration.GetLabel(invalid.Key)} is not a valid global hotkey.";
        }

        var duplicate = candidate.FindDuplicates().FirstOrDefault();
        return duplicate is null
            ? null
            : $"These actions use the same hotkey: {string.Join(", ", duplicate.Select(HotkeyConfiguration.GetLabel))}.";
    }

    private void RegisterHotkeys()
    {
        if (_isExiting || _suspendHotkeys)
        {
            return;
        }

        var failures = _hotkeys.ReplaceBindings(_settings.Hotkeys);
        var currentFailures = failures.ToHashSet();
        var newlyFailed = currentFailures
            .Where(action => !_reportedHotkeyFailures.Contains(action))
            .ToArray();
        _reportedHotkeyFailures.Clear();
        _reportedHotkeyFailures.UnionWith(currentFailures);
        if (newlyFailed.Length > 0)
        {
            _notifyIcon.ShowBalloonTip(
                5000,
                "Global hotkey unavailable",
                $"Could not register: {string.Join(", ", newlyFailed.Select(HotkeyConfiguration.GetLabel))}. Another app may be using the combination.",
                ToolTipIcon.Warning);
        }
    }

    private void HandleHotkeyPressed(object? sender, OverlayHotkeyAction action)
    {
        switch (action)
        {
            case OverlayHotkeyAction.ToggleLightEnhancement:
                _superLighter.ToggleEnabled();
                break;
            case OverlayHotkeyAction.OpenSuperLighterSettings:
                OpenDetailedSettings(_superLighter.OpenSettings);
                break;
            case OverlayHotkeyAction.ToggleAimoroReticle:
                _aimoro.ToggleOverlay();
                break;
            case OverlayHotkeyAction.CycleAimoroDisplays:
                _aimoro.CycleMonitor();
                break;
            case OverlayHotkeyAction.OpenAimoroSettings:
                OpenDetailedSettings(_aimoro.OpenSettings);
                break;
            case OverlayHotkeyAction.ToggleSoundDirectionOverlay:
                _soundDirection.ToggleOverlay();
                break;
            case OverlayHotkeyAction.CycleSoundDirectionDisplays:
                _soundDirection.CycleMonitor();
                break;
            case OverlayHotkeyAction.OpenSoundDirectionSettings:
                OpenDetailedSettings(_soundDirection.OpenSettings);
                break;
        }
    }

    private void HandleSuperLighterSaved(SuperSettings settings)
    {
        if (_applyingCanonicalHotkeys)
        {
            return;
        }

        var previous = _settings.Hotkeys.Clone();
        _settings.SuperLighter = settings.Clone();
        _settings.PullSuperLighterHotkeys();
        ReconcileModuleHotkeys(previous, () => _superLighter.SetHotkeys(
            ModuleHotkeyConversions.ToSuperLighter(previous.ToggleLightEnhancement),
            ModuleHotkeyConversions.ToSuperLighter(previous.OpenSuperLighterSettings)));
    }

    private void HandleAimoroSaved(AimoroSettings settings)
    {
        if (_applyingCanonicalHotkeys)
        {
            return;
        }

        var previous = _settings.Hotkeys.Clone();
        _settings.Aimoro = settings.Clone();
        _settings.PullAimoroHotkeys();
        ReconcileModuleHotkeys(previous, () => _aimoro.SetHotkeys(
            ModuleHotkeyConversions.ToAimoro(previous.ToggleAimoroReticle),
            ModuleHotkeyConversions.ToAimoro(previous.CycleAimoroDisplays),
            ModuleHotkeyConversions.ToAimoro(previous.OpenAimoroSettings)));
    }

    private void HandleSoundDirectionSaved(SoundSettings settings)
    {
        if (_applyingCanonicalHotkeys)
        {
            return;
        }

        var previous = _settings.Hotkeys.Clone();
        _settings.SoundDirectionVisualizer = settings.Clone();
        _settings.PullSoundDirectionHotkeys();
        ReconcileModuleHotkeys(previous, () => _soundDirection.SetHotkeys(
            ModuleHotkeyConversions.ToSoundDirection(previous.ToggleSoundDirectionOverlay),
            ModuleHotkeyConversions.ToSoundDirection(previous.CycleSoundDirectionDisplays),
            ModuleHotkeyConversions.ToSoundDirection(previous.OpenSoundDirectionSettings)));
    }

    private void ReconcileModuleHotkeys(HotkeyConfiguration previous, Action restoreModuleHotkeys)
    {
        var error = ValidateHotkeys(_settings.Hotkeys);
        if (error is not null)
        {
            _settings.Hotkeys = previous;
            _settings.ApplyCanonicalHotkeys();
            _applyingCanonicalHotkeys = true;
            try
            {
                restoreModuleHotkeys();
            }
            finally
            {
                _applyingCanonicalHotkeys = false;
            }

            MessageBox.Show(error, "Invalid hotkey", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        SaveSettings();
        RegisterHotkeys();
        UpdateUiState();
    }

    private void SaveSettings()
    {
        _settings.Normalize();
        _settingsStore.Save(_settings);
    }

    private void UpdateUiState()
    {
        _mainForm.UpdateState(_settings);
        _lightMenuItem.Checked = _settings.SuperLighter.Enabled;
        _aimoroMenuItem.Checked = _settings.Aimoro.OverlayEnabled;
        _soundMenuItem.Checked = _settings.SoundDirectionVisualizer.OverlayEnabled;
        _notifyIcon.Text = $"ModularGameOverlay - {new[] { _lightMenuItem.Checked, _aimoroMenuItem.Checked, _soundMenuItem.Checked }.Count(value => value)}/3 active";
    }

    public void ExitApplication()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;
        _startupTimer.Stop();
        _hotkeys.ClearBindings();
        _notifyIcon.Visible = false;
        _mainForm.AllowClose = true;
        _mainForm.Close();

        _soundDirection.ExitApplication();
        _soundDirection.Dispose();
        _aimoro.ExitApplication();
        _aimoro.Dispose();
        _superLighter.ExitApplication();
        _superLighter.Dispose();
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            RestoreDisplayEffects();
            _startupTimer.Stop();
            _openMainRegistration.Unregister(null);
            _hotkeys.HotkeyPressed -= HandleHotkeyPressed;
            _hotkeys.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _trayMenu.Dispose();
            _appIcon.Dispose();
            _mainForm.Dispose();
            _superSignal.Dispose();
            _aimoroSignal.Dispose();
            _soundSignal.Dispose();
            _startupTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private static void ConfigureTrayMenu(ContextMenuStrip menu)
    {
        menu.BackColor = DarkTheme.Card;
        menu.ForeColor = DarkTheme.Text;
        menu.ShowCheckMargin = true;
        menu.ShowImageMargin = false;
        foreach (ToolStripItem item in menu.Items)
        {
            item.ForeColor = DarkTheme.Text;
        }
    }
}
