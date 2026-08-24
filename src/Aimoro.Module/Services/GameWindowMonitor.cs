using Aimoro.App.Native;
using System.Diagnostics;

namespace Aimoro.App.Services;

public sealed class GameWindowMonitor
{
    private static readonly TimeSpan FullProcessScanInterval = TimeSpan.FromSeconds(3);
    private readonly SteamLibraryService _steamLibraryService;
    private DateTimeOffset _lastFullProcessScan = DateTimeOffset.MinValue;
    private IntPtr _lastDetectedWindowHandle;
    private int _lastDetectedProcessId;
    private DetectedGameTarget? _lastDetectedTarget;

    public GameWindowMonitor(SteamLibraryService steamLibraryService)
    {
        _steamLibraryService = steamLibraryService;
    }

    public DetectedGameTarget? Detect()
    {
        var gameInstallRoots = _steamLibraryService.GetGameInstallRoots();
        if (gameInstallRoots.Count == 0)
        {
            ClearCachedTarget();
            return null;
        }

        var foregroundWindow = NativeMethods.GetForegroundWindow();
        var cachedForegroundTarget = TryGetCachedTargetForWindow(foregroundWindow);
        if (cachedForegroundTarget is not null)
        {
            return cachedForegroundTarget;
        }

        var foregroundTarget = TryCreateTargetFromWindow(foregroundWindow, gameInstallRoots);
        if (foregroundTarget is not null)
        {
            CacheWindowTarget(foregroundWindow, foregroundTarget);
            return foregroundTarget;
        }

        var cachedTarget = TryGetCachedTarget();
        if (cachedTarget is not null)
        {
            return cachedTarget;
        }

        if (DateTimeOffset.UtcNow - _lastFullProcessScan < FullProcessScanInterval)
        {
            return null;
        }

        _lastFullProcessScan = DateTimeOffset.UtcNow;

        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                var target = TryCreateTargetFromProcess(process, gameInstallRoots);
                if (target is not null)
                {
                    CacheProcessTarget(process, target);
                    return target;
                }
            }
        }

        ClearCachedTarget();
        return null;
    }

    private DetectedGameTarget? TryGetCachedTargetForWindow(IntPtr windowHandle)
    {
        if (_lastDetectedTarget is null || windowHandle == IntPtr.Zero || windowHandle != _lastDetectedWindowHandle)
        {
            return null;
        }

        if (!IsViableWindow(windowHandle))
        {
            ClearCachedTarget();
            return null;
        }

        return _lastDetectedTarget with
        {
            Screen = Screen.FromHandle(windowHandle)
        };
    }

    private DetectedGameTarget? TryGetCachedTarget()
    {
        if (_lastDetectedTarget is null || _lastDetectedWindowHandle == IntPtr.Zero)
        {
            return null;
        }

        if (!IsViableWindow(_lastDetectedWindowHandle))
        {
            ClearCachedTarget();
            return null;
        }

        return _lastDetectedTarget with
        {
            Screen = Screen.FromHandle(_lastDetectedWindowHandle)
        };
    }

    private void CacheWindowTarget(IntPtr windowHandle, DetectedGameTarget target)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        _lastDetectedWindowHandle = windowHandle;
        NativeMethods.GetWindowThreadProcessId(windowHandle, out var processId);
        _lastDetectedProcessId = (int)processId;
        _lastDetectedTarget = target;
    }

    private void CacheProcessTarget(Process process, DetectedGameTarget target)
    {
        _lastDetectedProcessId = process.Id;
        _lastDetectedWindowHandle = process.MainWindowHandle;
        _lastDetectedTarget = target;
    }

    private void ClearCachedTarget()
    {
        _lastDetectedWindowHandle = IntPtr.Zero;
        _lastDetectedProcessId = 0;
        _lastDetectedTarget = null;
    }

    private static DetectedGameTarget? TryCreateTargetFromWindow(IntPtr windowHandle, IReadOnlyList<string> gameInstallRoots)
    {
        if (windowHandle == IntPtr.Zero || !IsViableWindow(windowHandle))
        {
            return null;
        }

        NativeMethods.GetWindowThreadProcessId(windowHandle, out var processId);
        if (processId == 0)
        {
            return null;
        }

        try
        {
            using var process = Process.GetProcessById((int)processId);
            return TryCreateTargetFromProcess(process, gameInstallRoots, windowHandle);
        }
        catch
        {
            return null;
        }
    }

    private static DetectedGameTarget? TryCreateTargetFromProcess(
        Process process,
        IReadOnlyList<string> gameInstallRoots,
        IntPtr? explicitHandle = null)
    {
        try
        {
            if (!explicitHandle.HasValue)
            {
                process.Refresh();
            }

            var windowHandle = explicitHandle ?? process.MainWindowHandle;

            if (windowHandle == IntPtr.Zero || !IsViableWindow(windowHandle))
            {
                return null;
            }

            var executablePath = process.MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                return null;
            }

            var normalizedPath = Path.GetFullPath(executablePath);
            var isSteamGame = gameInstallRoots.Any(root =>
                normalizedPath.StartsWith(root, StringComparison.OrdinalIgnoreCase));

            if (!isSteamGame)
            {
                return null;
            }

            return new DetectedGameTarget(
                process.ProcessName,
                process.MainWindowTitle,
                Screen.FromHandle(windowHandle));
        }
        catch
        {
            return null;
        }
    }

    private static bool IsViableWindow(IntPtr windowHandle)
    {
        return NativeMethods.IsWindowVisible(windowHandle) && !NativeMethods.IsIconic(windowHandle);
    }
}

public sealed record DetectedGameTarget(string ProcessName, string WindowTitle, Screen Screen);
