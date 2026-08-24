using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace Aimoro.App.Services;

public sealed class SteamLibraryService
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(5);
    private static readonly Regex VdfPathRegex = new("\"path\"\\s+\"(?<path>.+?)\"", RegexOptions.Compiled);
    private static readonly Regex LegacyLibraryRegex = new("^\\s*\"\\d+\"\\s+\"(?<path>.+?)\"\\s*$", RegexOptions.Compiled);
    private readonly object _syncRoot = new();
    private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;
    private IReadOnlyList<string>? _cachedLibraryRoots;

    public IReadOnlyList<string> GetLibraryRoots()
    {
        lock (_syncRoot)
        {
            if (_cachedLibraryRoots is not null && DateTimeOffset.UtcNow - _lastRefresh < CacheLifetime)
            {
                return _cachedLibraryRoots;
            }

            _cachedLibraryRoots = DiscoverLibraryRoots();
            _lastRefresh = DateTimeOffset.UtcNow;
            return _cachedLibraryRoots;
        }
    }

    public IReadOnlyList<string> GetGameInstallRoots()
    {
        return GetLibraryRoots()
            .Select(library => Path.Combine(library, "steamapps", "common"))
            .Where(Directory.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public void InvalidateCache()
    {
        lock (_syncRoot)
        {
            _cachedLibraryRoots = null;
            _lastRefresh = DateTimeOffset.MinValue;
        }
    }

    private static IReadOnlyList<string> DiscoverLibraryRoots()
    {
        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var installRoot in EnumerateSteamInstallCandidates())
        {
            if (!Directory.Exists(installRoot))
            {
                continue;
            }

            var normalizedRoot = NormalizePath(installRoot);
            results.Add(normalizedRoot);

            var libraryFoldersFile = Path.Combine(normalizedRoot, "steamapps", "libraryfolders.vdf");
            foreach (var libraryRoot in ParseLibraryFolders(libraryFoldersFile))
            {
                if (Directory.Exists(libraryRoot))
                {
                    results.Add(NormalizePath(libraryRoot));
                }
            }
        }

        return results.ToList();
    }

    private static IEnumerable<string> EnumerateSteamInstallCandidates()
    {
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in GetRegistryCandidates())
        {
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                candidates.Add(candidate);
            }
        }

        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        candidates.Add(Path.Combine(programFilesX86, "Steam"));
        candidates.Add(Path.Combine(programFiles, "Steam"));
        candidates.Add(Path.Combine(localAppData, "Programs", "Steam"));

        return candidates.Select(NormalizePath);
    }

    private static IEnumerable<string> GetRegistryCandidates()
    {
        using var currentUser = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
        yield return currentUser?.GetValue("SteamPath") as string ?? string.Empty;
        yield return currentUser?.GetValue("SteamExe") is string steamExe
            ? Path.GetDirectoryName(steamExe) ?? string.Empty
            : string.Empty;

        using var localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
            .OpenSubKey(@"SOFTWARE\Valve\Steam");
        yield return localMachine32?.GetValue("InstallPath") as string ?? string.Empty;

        using var wow6432 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
            .OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
        yield return wow6432?.GetValue("InstallPath") as string ?? string.Empty;
    }

    private static IEnumerable<string> ParseLibraryFolders(string filePath)
    {
        if (!File.Exists(filePath))
        {
            yield break;
        }

        foreach (var line in File.ReadLines(filePath))
        {
            var pathMatch = VdfPathRegex.Match(line);
            if (pathMatch.Success)
            {
                yield return NormalizePath(pathMatch.Groups["path"].Value);
                continue;
            }

            var legacyMatch = LegacyLibraryRegex.Match(line);
            if (legacyMatch.Success)
            {
                yield return NormalizePath(legacyMatch.Groups["path"].Value);
            }
        }
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path.Replace('/', '\\').Replace(@"\\", @"\"));
    }
}
