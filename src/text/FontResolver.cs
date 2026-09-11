using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace odl3d;

/// <summary>
/// Resolves font names or partial paths (e.g. "arial", "Roboto-Regular.ttf") to absolute font files.
/// Custom folders registered via AddSearchPath are searched first, in registration order, followed by the
/// platform's system font folders. All searches recurse into subfolders, which matters on Linux where fonts
/// are nested per-family. Successful and failed lookups are both cached, so call ClearCache after adding
/// fonts to a folder at runtime.
/// </summary>
public static class FontResolver
{
    private static readonly string[] Extensions = { ".ttf", ".otf", ".ttc", ".otc" };

    private static readonly List<string> _searchPaths = new();
    private static readonly Dictionary<string, string?> _resolved = new(StringComparer.OrdinalIgnoreCase);
    private static string[]? _systemSearchPaths;

    /// <summary>Custom folders searched before the system font folders, in the order they were added.</summary>
    public static IReadOnlyList<string> SearchPaths => _searchPaths;

    /// <summary>The platform's system font folders, searched after all custom SearchPaths.</summary>
    public static IReadOnlyList<string> SystemSearchPaths => _systemSearchPaths ??= BuildSystemSearchPaths();

    /// <summary>Registers a folder to search (recursively) before the system font folders.</summary>
    public static void AddSearchPath(string folder)
    {
        string full = Path.GetFullPath(folder);
        if (_searchPaths.Contains(full, StringComparer.OrdinalIgnoreCase)) return;
        _searchPaths.Add(full);
        ClearCache();
    }

    /// <summary>Unregisters a previously added custom search folder.</summary>
    public static void RemoveSearchPath(string folder)
    {
        string full = Path.GetFullPath(folder);
        if (_searchPaths.RemoveAll(p => string.Equals(p, full, StringComparison.OrdinalIgnoreCase)) > 0) ClearCache();
    }

    /// <summary>Unregisters all custom search folders. System font folders remain searchable.</summary>
    public static void ClearSearchPaths()
    {
        _searchPaths.Clear();
        ClearCache();
    }

    /// <summary>Forgets all previously resolved (and unresolvable) font names.</summary>
    public static void ClearCache() => _resolved.Clear();

    /// <summary>
    /// Resolves a font name or path to an absolute file path, throwing if no matching font file exists in any
    /// custom or system search folder.
    /// </summary>
    public static string Resolve(string nameOrPath)
    {
        if (TryResolve(nameOrPath, out string? path)) return path;
        throw new FileNotFoundException($"Could not find font '{nameOrPath}' in any custom or system font folder.", nameOrPath);
    }

    /// <summary>Resolves a font name or path to an absolute file path, returning false if it could not be found.</summary>
    public static bool TryResolve(string nameOrPath, out string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameOrPath);

        if (_resolved.TryGetValue(nameOrPath, out string? cached))
        {
            path = cached!;
            return cached != null;
        }

        string? found = Search(nameOrPath);
        _resolved[nameOrPath] = found;
        path = found!;
        return found != null;
    }

    private static string? Search(string nameOrPath)
    {
        if (File.Exists(nameOrPath)) return Path.GetFullPath(nameOrPath);
        // A name containing separators is a path, not something to look up by filename in the search folders.
        if (Path.IsPathRooted(nameOrPath) || nameOrPath.Contains('/') || nameOrPath.Contains('\\')) return null;

        string[] patterns = BuildPatterns(nameOrPath);

        foreach (string folder in _searchPaths)
        {
            string? match = SearchFolder(folder, patterns);
            if (match != null) return match;
        }
        foreach (string folder in SystemSearchPaths)
        {
            string? match = SearchFolder(folder, patterns);
            if (match != null) return match;
        }
        return null;
    }

    private static string[] BuildPatterns(string name)
    {
        string extension = Path.GetExtension(name);
        if (Array.Exists(Extensions, e => string.Equals(e, extension, StringComparison.OrdinalIgnoreCase)))
            return new[] { name };

        string[] patterns = new string[Extensions.Length];
        for (int i = 0; i < Extensions.Length; i++) patterns[i] = name + Extensions[i];
        return patterns;
    }

    private static string? SearchFolder(string folder, string[] patterns)
    {
        if (!Directory.Exists(folder)) return null;

        EnumerationOptions options = new()
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive
        };

        foreach (string pattern in patterns)
        {
            foreach (string file in Directory.EnumerateFiles(folder, pattern, options))
                return file;
        }
        return null;
    }

    private static string[] BuildSystemSearchPaths()
    {
        List<string> paths = new();

        if (OperatingSystem.IsWindows())
        {
            Add(paths, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts"));
            Add(paths, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Fonts"));
        }
        else if (OperatingSystem.IsMacOS())
        {
            Add(paths, "/System/Library/Fonts");
            Add(paths, "/Library/Fonts");
            Add(paths, "/Network/Library/Fonts");
            Add(paths, Path.Combine(Home(), "Library", "Fonts"));
        }
        else
        {
            Add(paths, "/usr/share/fonts");
            Add(paths, "/usr/local/share/fonts");
            Add(paths, "/usr/share/X11/fonts");
            Add(paths, Path.Combine(Home(), ".local", "share", "fonts"));
            Add(paths, Path.Combine(Home(), ".fonts"));
        }

        return paths.ToArray();

        static void Add(List<string> list, string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !list.Contains(path, StringComparer.OrdinalIgnoreCase)) list.Add(path);
        }

        static string Home() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
