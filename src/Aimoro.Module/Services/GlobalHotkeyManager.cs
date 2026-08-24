using Aimoro.App.Native;

namespace Aimoro.App.Services;

public enum HotkeyAction
{
    ToggleOverlay,
    CycleMonitor,
    OpenSettings
}

public sealed class GlobalHotkeyManager : IDisposable
{
    private readonly HotkeyMessageWindow _messageWindow = new();
    private readonly Dictionary<int, HotkeyRegistration> _registrations = new();
    private int _nextHotkeyId = 1;

    public GlobalHotkeyManager()
    {
        _messageWindow.HotkeyPressed += HandleWindowHotkeyPressed;
    }

    public event EventHandler<HotkeyAction>? HotkeyPressed;

    public void ReplaceBindings(IReadOnlyDictionary<HotkeyAction, HotkeyDefinition> bindings, out List<HotkeyAction> failures)
    {
        failures = new List<HotkeyAction>();
        ClearBindings();

        foreach (var binding in bindings)
        {
            if (binding.Value.IsEmpty)
            {
                continue;
            }

            if (!binding.Value.IsValid || !TryRegisterHotkey(binding.Key, binding.Value))
            {
                failures.Add(binding.Key);
            }
        }
    }

    public void ClearBindings()
    {
        foreach (var registration in _registrations)
        {
            _ = NativeMethods.UnregisterHotKey(_messageWindow.Handle, registration.Key);
        }

        _registrations.Clear();
        _nextHotkeyId = 1;
    }

    public void Dispose()
    {
        ClearBindings();
        _messageWindow.HotkeyPressed -= HandleWindowHotkeyPressed;
        _messageWindow.Dispose();
    }

    private void HandleWindowHotkeyPressed(object? sender, int hotkeyId)
    {
        if (_registrations.TryGetValue(hotkeyId, out var registration))
        {
            HotkeyPressed?.Invoke(this, registration.Action);
        }
    }

    private bool TryRegisterHotkey(HotkeyAction action, HotkeyDefinition definition)
    {
        var hotkeyId = _nextHotkeyId++;
        var modifiers = ToNativeModifiers(definition.Modifiers);
        var key = (uint)definition.Key;

        if (!NativeMethods.RegisterHotKey(_messageWindow.Handle, hotkeyId, modifiers | NativeMethods.MOD_NOREPEAT, key) &&
            !NativeMethods.RegisterHotKey(_messageWindow.Handle, hotkeyId, modifiers, key))
        {
            return false;
        }

        _registrations[hotkeyId] = new HotkeyRegistration(action);
        return true;
    }

    private static uint ToNativeModifiers(KeyModifiers modifiers)
    {
        var nativeModifiers = 0u;

        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            nativeModifiers |= NativeMethods.MOD_ALT;
        }

        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            nativeModifiers |= NativeMethods.MOD_CONTROL;
        }

        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            nativeModifiers |= NativeMethods.MOD_SHIFT;
        }

        if (modifiers.HasFlag(KeyModifiers.Windows))
        {
            nativeModifiers |= NativeMethods.MOD_WIN;
        }

        return nativeModifiers;
    }

    private sealed record HotkeyRegistration(HotkeyAction Action);

    private sealed class HotkeyMessageWindow : NativeWindow, IDisposable
    {
        public HotkeyMessageWindow()
        {
            CreateHandle(new CreateParams());
        }

        public event EventHandler<int>? HotkeyPressed;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_HOTKEY)
            {
                HotkeyPressed?.Invoke(this, m.WParam.ToInt32());
                return;
            }

            base.WndProc(ref m);
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                DestroyHandle();
            }
        }
    }
}
