using Aimoro.App.Services;
using Aimoro.App.UI;
using Aimoro.App.Native;
using Microsoft.Win32;
using System.Drawing;

namespace Aimoro.App;

public sealed class AimoroApplicationContext : ApplicationContext
{
    private readonly SettingsStore _settingsStore = new();
    private readonly SteamLibraryService _steamLibraryService = new();
    private readonly GlobalHotkeyManager _hotkeyManager = new();
    private readonly ReticleOverlayForm _overlayForm = new();
    private readonly Control _uiDispatcher = new();
    private readonly EventWaitHandle _openSettingsSignal;
    private readonly RegisteredWaitHandle _openSettingsSignalRegistration;
    private readonly System.Windows.Forms.Timer? _startupSettingsTimer;
    private readonly Icon _trayAppIcon;
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _trayMenu = new();
    private readonly ToolStripMenuItem _toggleOverlayMenuItem;
    private readonly ToolStripMenuItem _autoTargetMenuItem;
    private readonly ToolStripMenuItem _monitorsMenuItem;
    private readonly Dictionary<string, ToolStripMenuItem> _monitorMenuItems = new(StringComparer.OrdinalIgnoreCase);
    private readonly System.Windows.Forms.Timer _inputPollTimer = new() { Interval = 25 };
    private readonly GameWindowMonitor _gameWindowMonitor;
    private readonly NativeMethods.WinEventProc _foregroundWindowEventProc;
    private AppSettings _settings;
    private Screen _currentScreen;
    private DetectedGameTarget? _detectedGame;
    private SettingsForm? _activeSettingsForm;
    private bool _holdTriggerPressed;
    private bool _autoTargetRefreshInProgress;
    private bool _pendingAutoTargetRefresh;
    private bool _pendingAutoTargetForceRefresh;
    private bool _isExiting;
    private bool _overlaySettingsDirty = true;
    private IntPtr _foregroundWindowEventHook;
    private readonly Action<AppSettings>? _externalSave;
    private readonly bool _embeddedHost;

    public AimoroApplicationContext(
        EventWaitHandle openSettingsSignal,
        bool openSettingsOnStartup,
        AppSettings? initialSettings = null,
        Action<AppSettings>? externalSave = null,
        bool embeddedHost = false)
    {
        _externalSave = externalSave;
        _embeddedHost = embeddedHost;
        _openSettingsSignal = openSettingsSignal;
        _settings = initialSettings?.Clone() ?? _settingsStore.Load();
        _settings.Normalize();
        _gameWindowMonitor = new GameWindowMonitor(_steamLibraryService);
        _currentScreen = DisplayInfoFormatter.ResolveScreen(_settings.SelectedMonitorDeviceName);
        _holdTriggerPressed = IsConfiguredHoldButtonPressed();
        _trayAppIcon = LoadTrayIcon();
        _foregroundWindowEventProc = HandleForegroundWindowChanged;
        _ = _uiDispatcher.Handle;
        _openSettingsSignalRegistration = ThreadPool.RegisterWaitForSingleObject(
            _openSettingsSignal,
            static (state, _) => ((AimoroApplicationContext)state!).RequestOpenSettings(),
            this,
            Timeout.Infinite,
            false);

        _toggleOverlayMenuItem = new ToolStripMenuItem("Reticle Enabled", null, (_, _) => ToggleOverlay());
        _autoTargetMenuItem = new ToolStripMenuItem("Auto Target Steam Game Monitor", null, (_, _) => SetAutoTarget(!_settings.AutoDetectSteamGameMonitor));
        _monitorsMenuItem = new ToolStripMenuItem("Manual Monitor");
        var settingsMenuItem = new ToolStripMenuItem("Settings...", null, (_, _) => OpenSettings());
        var exitMenuItem = new ToolStripMenuItem("Exit", null, (_, _) => ExitApplication());

        _trayMenu.Items.AddRange(
            _toggleOverlayMenuItem,
            _autoTargetMenuItem,
            new ToolStripSeparator(),
            _monitorsMenuItem,
            new ToolStripSeparator(),
            settingsMenuItem,
            exitMenuItem);
        ConfigureTrayMenu();

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = _trayMenu,
            Icon = _trayAppIcon,
            Text = "Aimoro",
            Visible = !embeddedHost
        };

        _notifyIcon.DoubleClick += (_, _) => OpenSettings();
        _notifyIcon.MouseClick += HandleNotifyIconMouseClick;

        _hotkeyManager.HotkeyPressed += HandleHotkeyPressed;
        _inputPollTimer.Tick += (_, _) => PollInputState();
        SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;
        _foregroundWindowEventHook = NativeMethods.SetWinEventHook(
            NativeMethods.EVENT_SYSTEM_FOREGROUND,
            NativeMethods.EVENT_SYSTEM_FOREGROUND,
            IntPtr.Zero,
            _foregroundWindowEventProc,
            0,
            0,
            NativeMethods.WINEVENT_OUTOFCONTEXT | NativeMethods.WINEVENT_SKIPOWNPROCESS);

        RebuildMonitorMenu();
        RegisterHotkeys();
        RefreshTargetScreen(force: true);
        if (!embeddedHost || _settings.OverlayEnabled)
        {
            _inputPollTimer.Start();
        }

        if (openSettingsOnStartup && !embeddedHost)
        {
            _startupSettingsTimer = new System.Windows.Forms.Timer { Interval = 10 };
            _startupSettingsTimer.Tick += HandleStartupSettingsTimerTick;
            _startupSettingsTimer.Start();
        }
        else if (!embeddedHost)
        {
            ShowStartupHint();
        }
    }

    public AppSettings Settings => _settings.Clone();

    public bool IsEnabled => _settings.OverlayEnabled;

    private void HandleHotkeyPressed(object? sender, HotkeyAction action)
    {
        switch (action)
        {
            case HotkeyAction.ToggleOverlay:
                ToggleOverlay();
                break;
            case HotkeyAction.CycleMonitor:
                CycleMonitor();
                break;
            case HotkeyAction.OpenSettings:
                OpenSettings();
                break;
        }
    }

    public void ToggleOverlay()
    {
        _settings.OverlayEnabled = !_settings.OverlayEnabled;
        if (_embeddedHost)
        {
            if (_settings.OverlayEnabled)
            {
                _inputPollTimer.Start();
            }
            else
            {
                _inputPollTimer.Stop();
            }
        }

        PersistSettings();
        ApplyOverlayState();
        UpdateMenuState();
    }

    public void SetEnabled(bool enabled)
    {
        if (_settings.OverlayEnabled != enabled)
        {
            ToggleOverlay();
        }
    }

    private void SetAutoTarget(bool enabled)
    {
        _settings.AutoDetectSteamGameMonitor = enabled;
        PersistSettings();
        RefreshTargetScreen(force: true);
    }

    private void SelectManualMonitor(string deviceName)
    {
        _settings.AutoDetectSteamGameMonitor = false;
        _settings.SelectedMonitorDeviceName = deviceName;
        PersistSettings();
        RefreshTargetScreen(force: true);
    }

    public void CycleMonitor()
    {
        var screens = Screen.AllScreens;
        if (screens.Length == 0)
        {
            return;
        }

        var currentIndex = Array.FindIndex(screens, screen =>
            string.Equals(screen.DeviceName, _currentScreen.DeviceName, StringComparison.OrdinalIgnoreCase));

        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        var nextScreen = screens[(currentIndex + 1) % screens.Length];
        SelectManualMonitor(nextScreen.DeviceName);
    }

    public void OpenSettings()
    {
        if (_activeSettingsForm is not null && !_activeSettingsForm.IsDisposed)
        {
            if (_activeSettingsForm.WindowState == FormWindowState.Minimized)
            {
                _activeSettingsForm.WindowState = FormWindowState.Normal;
            }

            _activeSettingsForm.Activate();
            return;
        }

        _hotkeyManager.ClearBindings();
        _activeSettingsForm = new SettingsForm(_settings);
        _activeSettingsForm.SettingsChanged += HandleSettingsChanged;

        try
        {
            _activeSettingsForm.ShowDialog();
        }
        finally
        {
            _activeSettingsForm.SettingsChanged -= HandleSettingsChanged;
            _activeSettingsForm.Dispose();
            _activeSettingsForm = null;
            RegisterHotkeys();
        }
    }

    private void HandleSettingsChanged(object? sender, EventArgs e)
    {
        if (sender is not SettingsForm settingsForm)
        {
            return;
        }

        _settings = settingsForm.ResultSettings.Clone();
        _holdTriggerPressed = IsConfiguredHoldButtonPressed();
        _overlaySettingsDirty = true;
        PersistSettings();
        RebuildMonitorMenu();
        ApplyOverlayState();
        RefreshTargetScreen(force: true);
    }

    private void RequestOpenSettings()
    {
        try
        {
            if (_uiDispatcher.IsDisposed)
            {
                return;
            }

            _uiDispatcher.BeginInvoke(new MethodInvoker(OpenSettings));
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void RegisterHotkeys()
    {
        if (_embeddedHost)
        {
            return;
        }

        var bindings = new Dictionary<HotkeyAction, HotkeyDefinition>
        {
            [HotkeyAction.ToggleOverlay] = _settings.ToggleHotkey,
            [HotkeyAction.CycleMonitor] = _settings.CycleMonitorHotkey,
            [HotkeyAction.OpenSettings] = _settings.OpenSettingsHotkey
        };

        _hotkeyManager.ReplaceBindings(bindings, out var failures);
        if (failures.Count == 0)
        {
            return;
        }

        var failedLabels = failures.Select(action => action switch
        {
            HotkeyAction.ToggleOverlay => "Toggle overlay",
            HotkeyAction.CycleMonitor => "Cycle monitor",
            HotkeyAction.OpenSettings => "Open settings",
            _ => action.ToString()
        });

        _notifyIcon.ShowBalloonTip(
            3000,
            "Aimoro hotkey warning",
            $"Could not register: {string.Join(", ", failedLabels)}. Another app is probably using that key combination.",
            ToolTipIcon.Warning);
    }

    private void RefreshTargetScreen(bool force = false)
    {
        if (_isExiting)
        {
            return;
        }

        if (!_settings.AutoDetectSteamGameMonitor)
        {
            _detectedGame = null;
            ApplyResolvedScreen(DisplayInfoFormatter.ResolveScreen(_settings.SelectedMonitorDeviceName), force);
            return;
        }

        QueueAutoTargetRefresh(force);
    }

    private void ApplyResolvedScreen(Screen resolvedScreen, bool force)
    {
        var changed = force || !string.Equals(resolvedScreen.DeviceName, _currentScreen.DeviceName, StringComparison.OrdinalIgnoreCase);
        _currentScreen = resolvedScreen;

        if (!_settings.AutoDetectSteamGameMonitor)
        {
            _settings.SelectedMonitorDeviceName = _currentScreen.DeviceName;
        }

        if (changed)
        {
            _overlayForm.SetTargetScreen(_currentScreen);
        }

        ApplyOverlayState(invalidateVisibleOverlay: force);
        UpdateMenuState();
    }

    private void QueueAutoTargetRefresh(bool force)
    {
        if (_isExiting)
        {
            return;
        }

        _pendingAutoTargetRefresh = true;
        _pendingAutoTargetForceRefresh |= force;

        if (_autoTargetRefreshInProgress)
        {
            return;
        }

        _ = RefreshAutoTargetScreenAsync();
    }

    private async Task RefreshAutoTargetScreenAsync()
    {
        if (_autoTargetRefreshInProgress)
        {
            return;
        }

        _autoTargetRefreshInProgress = true;

        try
        {
            while (_pendingAutoTargetRefresh)
            {
                var force = _pendingAutoTargetForceRefresh;
                _pendingAutoTargetRefresh = false;
                _pendingAutoTargetForceRefresh = false;

                var detectedGame = await Task.Run(() =>
                {
                    try
                    {
                        return _gameWindowMonitor.Detect();
                    }
                    catch
                    {
                        return null;
                    }
                });

                if (_isExiting || _uiDispatcher.IsDisposed)
                {
                    return;
                }

                if (!_settings.AutoDetectSteamGameMonitor)
                {
                    _detectedGame = null;
                    ApplyResolvedScreen(DisplayInfoFormatter.ResolveScreen(_settings.SelectedMonitorDeviceName), true);
                    continue;
                }

                _detectedGame = detectedGame;
                ApplyResolvedScreen(detectedGame?.Screen ?? _currentScreen, force);
            }
        }
        finally
        {
            _autoTargetRefreshInProgress = false;

            if (!_isExiting && _pendingAutoTargetRefresh)
            {
                _ = RefreshAutoTargetScreenAsync();
            }
        }
    }

    private void ApplyOverlayState(bool invalidateVisibleOverlay = true)
    {
        var overlaySettingsDirty = _overlaySettingsDirty;
        if (overlaySettingsDirty)
        {
            _overlayForm.ApplySettings(_settings);
            _overlaySettingsDirty = false;
        }

        if (ShouldShowOverlay())
        {
            if (!_overlayForm.Visible)
            {
                _overlayForm.Show();
            }
            else if (overlaySettingsDirty || invalidateVisibleOverlay)
            {
                _overlayForm.Invalidate();
            }
        }
        else if (_overlayForm.Visible)
        {
            _overlayForm.Hide();
        }
    }

    private bool ShouldShowOverlay()
    {
        if (!_settings.OverlayEnabled)
        {
            return false;
        }

        if (!_settings.HoldToShowEnabled)
        {
            return true;
        }

        return _holdTriggerPressed;
    }

    private void PollInputState()
    {
        var holdTriggerPressed = IsConfiguredHoldButtonPressed();
        if (holdTriggerPressed == _holdTriggerPressed)
        {
            return;
        }

        _holdTriggerPressed = holdTriggerPressed;
        ApplyOverlayState();
    }

    private bool IsConfiguredHoldButtonPressed()
    {
        var virtualKey = _settings.HoldToShowMouseButton switch
        {
            HoldToShowMouseButton.LeftButton => NativeMethods.VK_LBUTTON,
            HoldToShowMouseButton.RightButton => NativeMethods.VK_RBUTTON,
            HoldToShowMouseButton.MiddleButton => NativeMethods.VK_MBUTTON,
            HoldToShowMouseButton.XButton1 => NativeMethods.VK_XBUTTON1,
            HoldToShowMouseButton.XButton2 => NativeMethods.VK_XBUTTON2,
            _ => NativeMethods.VK_RBUTTON
        };

        return (NativeMethods.GetAsyncKeyState(virtualKey) & 0x8000) != 0;
    }

    private void HandleForegroundWindowChanged(
        IntPtr hWinEventHook,
        uint eventType,
        IntPtr hwnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime)
    {
        if (_isExiting || !_settings.AutoDetectSteamGameMonitor || _uiDispatcher.IsDisposed)
        {
            return;
        }

        try
        {
            _uiDispatcher.BeginInvoke(new MethodInvoker(() => RefreshTargetScreen(force: true)));
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void PersistSettings()
    {
        _settings.Normalize();
        if (_externalSave is not null)
        {
            _externalSave(_settings.Clone());
        }
        else
        {
            _settingsStore.Save(_settings);
        }
    }

    public void SetHotkeys(
        HotkeyDefinition toggle,
        HotkeyDefinition cycleMonitor,
        HotkeyDefinition openSettings)
    {
        _settings.ToggleHotkey = toggle.Clone();
        _settings.CycleMonitorHotkey = cycleMonitor.Clone();
        _settings.OpenSettingsHotkey = openSettings.Clone();
        PersistSettings();
    }

    private void RebuildMonitorMenu()
    {
        _monitorMenuItems.Clear();
        _monitorsMenuItem.DropDownItems.Clear();

        foreach (var screen in Screen.AllScreens)
        {
            var item = new ToolStripMenuItem(DisplayInfoFormatter.ToDisplayLabel(screen), null, (_, _) => SelectManualMonitor(screen.DeviceName))
            {
                ForeColor = DarkUiTheme.PrimaryText,
                Tag = screen.DeviceName
            };

            _monitorMenuItems[screen.DeviceName] = item;
            _monitorsMenuItem.DropDownItems.Add(item);
        }

        _monitorsMenuItem.DropDown.BackColor = DarkUiTheme.CardBackground;
        _monitorsMenuItem.DropDown.ForeColor = DarkUiTheme.PrimaryText;
        _monitorsMenuItem.DropDown.Renderer = _trayMenu.Renderer;
    }

    private void ConfigureTrayMenu()
    {
        _trayMenu.BackColor = DarkUiTheme.CardBackground;
        _trayMenu.ForeColor = DarkUiTheme.PrimaryText;
        _trayMenu.Renderer = new ToolStripProfessionalRenderer(new DarkToolStripColorTable());
        _trayMenu.ShowCheckMargin = true;
        _trayMenu.ShowImageMargin = false;

        foreach (ToolStripItem item in _trayMenu.Items)
        {
            item.ForeColor = DarkUiTheme.PrimaryText;
        }
    }

    private void UpdateMenuState()
    {
        _toggleOverlayMenuItem.Checked = _settings.OverlayEnabled;
        _autoTargetMenuItem.Checked = _settings.AutoDetectSteamGameMonitor;

        foreach (var entry in _monitorMenuItems)
        {
            entry.Value.Checked = !_settings.AutoDetectSteamGameMonitor &&
                string.Equals(entry.Key, _currentScreen.DeviceName, StringComparison.OrdinalIgnoreCase);
        }

        var statusText = _settings.AutoDetectSteamGameMonitor && _detectedGame is not null
            ? $"Aimoro - Auto on {DisplayInfoFormatter.ToDisplayLabel(_currentScreen)}"
            : $"Aimoro - {DisplayInfoFormatter.ToDisplayLabel(_currentScreen)}";

        _notifyIcon.Text = statusText.Length > 63 ? statusText[..63] : statusText;
    }

    private void ShowStartupHint()
    {
        var modeText = _settings.HoldToShowEnabled
            ? $"Hold {_settings.HoldToShowMouseButton.ToDisplayString().ToLowerInvariant()} to show the reticle."
            : "Reticle stays visible while enabled.";

        _notifyIcon.ShowBalloonTip(
            4000,
            "Aimoro is running",
            $"Toggle: {_settings.ToggleHotkey.ToDisplayString()} | Settings: {_settings.OpenSettingsHotkey.ToDisplayString()}. {modeText}",
            ToolTipIcon.Info);
    }

    private void HandleNotifyIconMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            OpenSettings();
        }
    }

    private void HandleStartupSettingsTimerTick(object? sender, EventArgs e)
    {
        if (_startupSettingsTimer is null)
        {
            return;
        }

        _startupSettingsTimer.Stop();
        _startupSettingsTimer.Tick -= HandleStartupSettingsTimerTick;
        OpenSettings();
    }

    private static Icon LoadTrayIcon()
    {
        try
        {
            var extractedIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (extractedIcon is not null)
            {
                return (Icon)extractedIcon.Clone();
            }
        }
        catch
        {
        }

        return (Icon)SystemIcons.Application.Clone();
    }

    private void HandleDisplaySettingsChanged(object? sender, EventArgs e)
    {
        RebuildMonitorMenu();
        RefreshTargetScreen(force: true);
    }

    public void ExitApplication()
    {
        _isExiting = true;
        _notifyIcon.Visible = false;
        ExitThread();
    }

    protected override void ExitThreadCore()
    {
        _isExiting = true;
        _openSettingsSignalRegistration.Unregister(null);
        _startupSettingsTimer?.Stop();
        if (_startupSettingsTimer is not null)
        {
            _startupSettingsTimer.Tick -= HandleStartupSettingsTimerTick;
            _startupSettingsTimer.Dispose();
        }
        _inputPollTimer.Stop();
        _inputPollTimer.Dispose();
        if (_foregroundWindowEventHook != IntPtr.Zero)
        {
            _ = NativeMethods.UnhookWinEvent(_foregroundWindowEventHook);
            _foregroundWindowEventHook = IntPtr.Zero;
        }
        _hotkeyManager.Dispose();
        _overlayForm.Dispose();
        _uiDispatcher.Dispose();
        _notifyIcon.Dispose();
        _trayAppIcon.Dispose();
        _trayMenu.Dispose();
        SystemEvents.DisplaySettingsChanged -= HandleDisplaySettingsChanged;
        base.ExitThreadCore();
    }
}
