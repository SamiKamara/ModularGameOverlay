using System.Runtime.InteropServices;

namespace ModularGameOverlay.App.Hotkeys;

internal sealed class GlobalHotkeyManager : NativeWindow, IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModWindows = 0x0008;
    private const uint ModNoRepeat = 0x4000;

    private readonly Dictionary<int, OverlayHotkeyAction> _registrations = [];
    private bool _disposed;

    public GlobalHotkeyManager()
    {
        CreateHandle(new CreateParams { Caption = "ModularGameOverlay.Hotkeys" });
    }

    public event EventHandler<OverlayHotkeyAction>? HotkeyPressed;

    public IReadOnlyList<OverlayHotkeyAction> ReplaceBindings(HotkeyConfiguration configuration)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ClearBindings();

        var failures = new List<OverlayHotkeyAction>();
        var duplicates = configuration.FindDuplicates().SelectMany(group => group).ToHashSet();
        var nextId = 1;
        foreach (var (action, binding) in configuration.GetBindings())
        {
            if (binding.IsEmpty)
            {
                continue;
            }

            if (!binding.IsValid || duplicates.Contains(action) || !TryRegister(nextId, binding))
            {
                failures.Add(action);
                continue;
            }

            _registrations[nextId] = action;
            nextId++;
        }

        return failures;
    }

    public void ClearBindings()
    {
        foreach (var id in _registrations.Keys)
        {
            _ = UnregisterHotKey(Handle, id);
        }

        _registrations.Clear();
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == WmHotkey &&
            _registrations.TryGetValue(message.WParam.ToInt32(), out var action))
        {
            HotkeyPressed?.Invoke(this, action);
        }

        base.WndProc(ref message);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        ClearBindings();
        DestroyHandle();
    }

    private bool TryRegister(int id, HotkeyBinding binding)
    {
        var modifiers = ModNoRepeat;
        if (binding.Modifiers.HasFlag(HotkeyModifiers.Control)) modifiers |= ModControl;
        if (binding.Modifiers.HasFlag(HotkeyModifiers.Alt)) modifiers |= ModAlt;
        if (binding.Modifiers.HasFlag(HotkeyModifiers.Shift)) modifiers |= ModShift;
        if (binding.Modifiers.HasFlag(HotkeyModifiers.Windows)) modifiers |= ModWindows;
        return RegisterHotKey(Handle, id, modifiers, (uint)binding.Key);
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr window, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr window, int id);
}
