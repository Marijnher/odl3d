using System.Runtime.InteropServices;
using odl3d.Renderer;
using Xunit;

namespace odl3d.Tests;

public sealed class NativeInteropCollection
{
    public const string Name = "Native interop";
}

[CollectionDefinition(NativeInteropCollection.Name, DisableParallelization = true)]
public sealed class NativeInteropCollectionDefinition { }

[Collection(NativeInteropCollection.Name)]
[Trait("Tier", "T1")]
public class FreeTypeInteropTests
{
    [Fact]
    public void FreeType_loads_faces_measures_and_rasterizes_glyphs()
    {
        string fontPath = FindSystemFont();
        FT.Load();
        FT.Load();
        Assert.True(FT.Loaded);

        for (int iteration = 0; iteration < 10; iteration++)
        {
            Assert.Equal(0, FT.FT_Init_FreeType(out IntPtr library));
            Assert.NotEqual(IntPtr.Zero, library);
            Assert.Equal(0, FT.FT_Done_FreeType(library));
        }

        using Font font = Font.Get(fontPath, 24);
        Assert.True(font.Ascender > 0);
        Assert.True(font.LineHeight > 0);
        Assert.True(float.IsFinite(font.MeasureLine("Native interop")));
        Assert.True(float.IsFinite(font.GetKerning('A', 'V')));

        RasterizedGlyph glyph = font.RasterizeCodepoint('A', FontStyle.Regular);
        Assert.True(glyph.Width > 0);
        Assert.True(glyph.Height > 0);
        Assert.Equal(glyph.Width * glyph.Height, glyph.Coverage.Length);

        GlyphOutline outline = font.GetGlyphOutline('O', FontStyle.Regular);
        Assert.True(outline.Contours.Count > 0);
        Assert.True(font.RasterizeCodepoint('A', FontStyle.Italic).Width > 0);
        Assert.True(font.RasterizeCodepoint('A', FontStyle.Bold).Width > 0);
        Assert.True(font.RasterizeCodepoint(' ', FontStyle.Regular).Advance >= 0);
        font.Dispose();
        Assert.True(font.Disposed);
    }

    [Fact]
    public void FreeType_rejects_a_missing_face_with_a_managed_exception()
    {
        FT.Load();
        string missingPath = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.ttf");

        Assert.Throws<FileNotFoundException>(() => Font.Get(missingPath, 16));
    }

    private static string FindSystemFont()
    {
        foreach (string directory in FontResolver.SystemSearchPaths)
        {
            if (!Directory.Exists(directory)) continue;
            string? font = Directory.EnumerateFiles(directory, "*.ttf", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (font != null) return font;
        }

        throw new FileNotFoundException("No system TrueType font is installed for the FreeType interop smoke test.");
    }
}

[Collection(NativeInteropCollection.Name)]
[Trait("Tier", "T1")]
public class GlfwInteropTests
{
    [RequiresDisplayFact]
    public void Initializes_reports_supported_version_and_creates_hidden_window()
    {
        GLFW.Load();
        GLFW.Load();
        Assert.True(GLFW.Loaded);

        GLFW.glfwGetVersion(out int major, out int minor, out _);
        Assert.True(major > 3 || major == 3 && minor >= 3);

        GLFW.glfwWindowHint(GLFW.GLFW_VISIBLE, GLFW.GLFW_FALSE);
        GLFW.glfwWindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_NO_API);
        IntPtr window = GLFW.glfwCreateWindow(1, 1, "odl3d native test", IntPtr.Zero, IntPtr.Zero);
        try
        {
            Assert.NotEqual(IntPtr.Zero, window);
        }
        finally
        {
            if (window != IntPtr.Zero) GLFW.glfwDestroyWindow(window);
            GLFW.Terminate();
            Assert.True(GLFW.Loaded);
            GLFW.Terminate();
        }

        Assert.False(GLFW.Loaded);
    }
}

[Collection(NativeInteropCollection.Name)]
[Trait("Tier", "T1")]
public class MetalInteropTests
{
    [Fact]
    public void Metal_binding_is_guarded_by_platform()
    {
        if (OperatingSystem.IsMacOS())
        {
            Metal.Load();
            Assert.True(Metal.Loaded);
            return;
        }

        Assert.Throws<PlatformNotSupportedException>(Metal.Load);
    }
}

public sealed class RequiresDisplayFactAttribute : FactAttribute
{
    public RequiresDisplayFactAttribute()
    {
        if (OperatingSystem.IsLinux()
            && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY"))
            && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
        {
            Skip = "GLFW window tests require a display; use Xvfb on headless Linux.";
        }
        // GLFW's Cocoa backend requires the main thread and a logged-in window-server session;
        // hosted macOS CI runners hang in glfwInit()/glfwCreateWindow without one.
        else if (OperatingSystem.IsMacOS() && Environment.GetEnvironmentVariable("CI") != null)
        {
            Skip = "GLFW window tests hang without an interactive window-server session on hosted macOS CI runners.";
        }
    }
}

[Collection(NativeInteropCollection.Name)]
[Trait("Tier", "T1")]
public class NativeLibraryResolverTests
{
    [Fact]
    public void Loads_from_system_search_and_reports_missing_libraries_and_exports()
    {
        string[] systemLibrary = OperatingSystem.IsWindows() ? ["kernel32.dll"] :
            OperatingSystem.IsMacOS() ? ["libSystem.B.dylib"] : ["libc.so.6"];
        IntPtr library = NativeLibraryResolver.LoadFromBinOrSystem(systemLibrary, "system library not found");
        try
        {
            Assert.Throws<EntryPointNotFoundException>(() =>
                NativeLibraryResolver.GetFunction<Action>(library, "odl3d_missing_native_export"));
        }
        finally
        {
            NativeLibrary.Free(library);
        }

        Assert.Throws<DllNotFoundException>(() => NativeLibraryResolver.LoadFromBinOrSystem(
            [$"odl3d-missing-{Guid.NewGuid():N}.dll"], "native library not found"));
        Assert.Throws<RenderException>(() => GLFW.EnsureInitSucceeded(GLFW.GLFW_FALSE));
        GLFW.EnsureInitSucceeded(GLFW.GLFW_TRUE);
    }
}