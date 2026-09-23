using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace odl3d;

/// <summary>
/// A font family loaded via FreeType at a single fixed pixel size, providing Unicode-aware measurement and
/// glyph rasterization. Regular is required; Bold/Italic/BoldItalic face files are optional. When a style is
/// requested but no dedicated face was supplied for it, the regular face is used with a synthesized effect
/// (embolden for Bold, shear for Italic) instead.
/// Instances are obtained through Font.Get, which resolves face names through FontResolver and returns an
/// already-loaded font whenever one with identical faces and pixel size exists.
/// </summary>
public sealed unsafe class Font : IDisposable
{
    private readonly record struct FaceHandle(IntPtr Face, IntPtr GlyphSlot);

    private readonly record struct FontKey(string Regular, int PixelSize, string? Bold, string? Italic, string? BoldItalic);

    private static readonly Dictionary<FontKey, Font> _cache = new();

    /// <summary>Maximum deviation, in font pixels, allowed when flattening glyph curves into line segments.</summary>
    private const float OutlineFlatness = 0.1f;

    private readonly Dictionary<(int Codepoint, FontStyle Style), GlyphOutline> _outlineCache = new();

    private static IntPtr _ftLibrary;

    private static void EnsureLibrary()
    {
        if (_ftLibrary != IntPtr.Zero) return;
        FT.Load();
        if (FT.FT_Init_FreeType(out _ftLibrary) != 0)
            throw new InvalidOperationException("Failed to initialize FreeType.");
    }

    private readonly FaceHandle _regular;
    private readonly FaceHandle? _bold;
    private readonly FaceHandle? _italic;
    private readonly FaceHandle? _boldItalic;

    /// <summary>
    /// The fixed pixel size at which this font rasterizes and measures glyphs.
    /// </summary>
    public int PixelSize { get; }

    /// <summary>
    /// Distance from the baseline to the top of the font's tallest glyphs, in pixels.
    /// </summary>
    public float Ascender { get; }

    /// <summary>
    /// Distance from the baseline to the bottom of the font's lowest-hanging glyphs, in pixels (positive).
    /// </summary>
    public float Descender { get; }

    /// <summary>
    /// Recommended distance between baselines of consecutive lines, in pixels.
    /// </summary>
    public float LineHeight { get; }

    /// <summary>
    /// Distance from the baseline to where an underline should be drawn, in pixels (positive, downward).
    /// </summary>
    public float UnderlinePosition { get; }

    /// <summary>
    /// Recommended thickness of an underline/strikethrough bar, in pixels.
    /// </summary>
    public float UnderlineThickness { get; }

    public bool Disposed { get; private set; }

    private readonly FontKey _key;

    /// <summary>
    /// Returns a font with the given faces and pixel size, loading it only if an identical one has not been
    /// loaded (and not yet disposed) before. Face names are resolved through FontResolver, so they may be a
    /// full path or a bare family/file name such as "arial" to look up in the custom and system font folders.
    /// </summary>
    /// <param name="regular">Name or path of the regular-weight font file (.ttf/.otf/.ttc). Required.</param>
    /// <param name="pixelSize">Fixed pixel size to rasterize and measure glyphs at.</param>
    /// <param name="bold">Optional dedicated bold face; falls back to synthetic embolden if omitted.</param>
    /// <param name="italic">Optional dedicated italic face; falls back to synthetic shear if omitted.</param>
    /// <param name="boldItalic">Optional dedicated bold-italic face; falls back to synthesizing both if omitted.</param>
    public static Font Get(string regular, int pixelSize, string? bold = null, string? italic = null, string? boldItalic = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pixelSize, 1);

        FontKey key = new(
            FontResolver.Resolve(regular),
            pixelSize,
            bold != null ? FontResolver.Resolve(bold) : null,
            italic != null ? FontResolver.Resolve(italic) : null,
            boldItalic != null ? FontResolver.Resolve(boldItalic) : null);

        if (_cache.TryGetValue(key, out Font? cached)) return cached;

        Font font = new Font(key);
        _cache[key] = font;
        return font;
    }

    /// <summary>
    /// Disposes every cached font. Any Text still referencing one of them must be rebuilt afterwards.
    /// </summary>
    public static void ClearCache()
    {
        foreach (Font font in new List<Font>(_cache.Values)) font.Dispose();
        _cache.Clear();
    }

    private Font(FontKey key)
    {
        EnsureLibrary();
        _key = key;
        PixelSize = key.PixelSize;

        _regular = LoadFace(key.Regular, PixelSize);
        _bold = key.Bold != null ? LoadFace(key.Bold, PixelSize) : null;
        _italic = key.Italic != null ? LoadFace(key.Italic, PixelSize) : null;
        _boldItalic = key.BoldItalic != null ? LoadFace(key.BoldItalic, PixelSize) : null;

        (float ascender, float descender, float height) = FT.GetSizeMetrics(FT.GetFaceSize(_regular.Face));
        Ascender = ascender;
        Descender = -descender; // FreeType reports descender as negative; expose as a positive magnitude
        LineHeight = height;
        UnderlinePosition = -FT.GetFaceUnderlinePosition(_regular.Face);
        UnderlineThickness = Math.Max(1f, FT.GetFaceUnderlineThickness(_regular.Face));
    }

    private static FaceHandle LoadFace(string path, int pixelSize)
    {
        if (FT.FT_New_Face(_ftLibrary, path, 0, out IntPtr face) != 0)
            throw new FileNotFoundException($"Failed to load font face from '{path}'.");
        FT.FT_Set_Pixel_Sizes(face, 0, (uint)pixelSize);
        return new FaceHandle(face, FT.GetFaceGlyphSlot(face));
    }

    private (FaceHandle handle, bool syntheticBold, bool syntheticItalic) ResolveFace(FontStyle style)
    {
        bool bold = (style & FontStyle.Bold) != 0;
        bool italic = (style & FontStyle.Italic) != 0;

        if (bold && italic && _boldItalic is { } bi) return (bi, false, false);
        if (bold && !italic && _bold is { } b) return (b, false, false);
        if (italic && !bold && _italic is { } i) return (i, false, false);
        return (_regular, bold, italic);
    }

    private long SyntheticBoldStrength() => (long)(PixelSize / 24.0 * 64);

    /// <summary>
    /// Applies the transform (for synthetic italic) and loads the glyph outline (with synthetic embolden for bold applied to the outline, before any rasterization happens). Does not render a bitmap.
    /// </summary>
    /// <param name="handle">The font face handle to load the glyph from.</param>
    /// <param name="glyphIndex">The index of the glyph to load.</param>
    /// <param name="syntheticBold">Whether to apply synthetic bold to the glyph outline.</param>
    /// <param name="syntheticItalic">Whether to apply synthetic italic to the glyph outline.</param>
    /// <param name="boldStrength">The strength of the synthetic bold applied to the glyph outline.</param>
    private void LoadGlyph(FaceHandle handle, uint glyphIndex, bool syntheticBold, bool syntheticItalic, out long boldStrength)
    {
        boldStrength = 0;

        if (syntheticItalic)
        {
            byte* matrix = stackalloc byte[FT.MatrixBufferSize];
            FT.WriteMatrix((IntPtr)matrix, 1.0, 0.22, 0.0, 1.0);
            FT.FT_Set_Transform(handle.Face, (IntPtr)matrix, IntPtr.Zero);
        }
        else
        {
            FT.FT_Set_Transform(handle.Face, IntPtr.Zero, IntPtr.Zero);
        }

        FT.FT_Load_Glyph(handle.Face, glyphIndex, FT.FT_LOAD_DEFAULT);

        if (syntheticBold)
        {
            boldStrength = SyntheticBoldStrength();
            FT.FT_Outline_Embolden(FT.GetOutlinePtr(handle.GlyphSlot), boldStrength);
        }
    }

    private static float GetKerning(IntPtr face, uint left, uint right)
    {
        if (left == 0 || right == 0) return 0f;
        byte* buffer = stackalloc byte[FT.KerningBufferSize];
        FT.FT_Get_Kerning(face, left, right, FT.FT_KERNING_DEFAULT, (IntPtr)buffer);
        return FT.ReadKerningX((IntPtr)buffer);
    }

    /// <summary>
    /// Kerning adjustment (in pixels, positive or negative) to apply between two adjacent codepoints in this style, e.g. when laying out glyphs one at a time to match the pen positions MeasureString accounts for.
    /// </summary>
    /// <param name="leftCodepoint">The Unicode codepoint of the left character.</param>
    /// <param name="rightCodepoint">The Unicode codepoint of the right character.</param>
    /// <param name="style">The font style to use when measuring kerning.</param>
    public float GetKerning(int leftCodepoint, int rightCodepoint, FontStyle style = FontStyle.Regular)
    {
        (FaceHandle handle, _, _) = ResolveFace(style);
        uint left = FT.FT_Get_Char_Index(handle.Face, (nuint)leftCodepoint);
        uint right = FT.FT_Get_Char_Index(handle.Face, (nuint)rightCodepoint);
        return GetKerning(handle.Face, left, right);
    }

    /// <summary>
    /// Splits text into its individual lines, treating \n, \r\n and \r all as line breaks. Always returns at
    /// least one (possibly empty) line, so an empty string still occupies a single line's height.
    /// </summary>
    /// <param name="text">The text to split into lines.</param>
    /// <returns>An array of strings, each representing a line of text.</returns>
    public static string[] SplitLines(string text) => text.ReplaceLineEndings("\n").Split('\n');

    /// <summary>
    /// Measures the size of a block of text in the given style, without rasterizing or drawing anything. Line
    /// breaks are honored: the width is that of the widest line and the height is one LineHeight per line.
    /// Iterates by Unicode scalar value, so text outside the BMP (e.g. emoji) is measured correctly.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="style">The font style to use when measuring the text.</param>
    /// <returns>A Vector2 representing the width and height of the text block.</returns>
    public Vector2 MeasureString(string text, FontStyle style = FontStyle.Regular)
    {
        string[] lines = SplitLines(text);
        float width = 0f;
        foreach (string line in lines) width = Math.Max(width, MeasureLine(line, style));
        return new Vector2(width, lines.Length * LineHeight);
    }

    /// <summary>
    /// Measures the advance width of a single line of text in the given style. Any line breaks in the input
    /// are measured as ordinary glyphs, so callers with multi-line text should use MeasureString instead.
    /// </summary>
    /// <param name="line">The line of text to measure.</param>
    /// <param name="style">The font style to use when measuring the line.</param>
    /// <returns>The advance width of the line in pixels.</returns>
    public float MeasureLine(string line, FontStyle style = FontStyle.Regular)
    {
        (FaceHandle handle, bool syntheticBold, bool syntheticItalic) = ResolveFace(style);
        float width = 0f;
        uint previousGlyph = 0;
        bool hasPrevious = false;

        foreach (Rune rune in line.EnumerateRunes())
        {
            uint glyphIndex = FT.FT_Get_Char_Index(handle.Face, (nuint)rune.Value);
            if (hasPrevious) width += GetKerning(handle.Face, previousGlyph, glyphIndex);

            LoadGlyph(handle, glyphIndex, syntheticBold, syntheticItalic, out long boldStrength);
            width += FT.GetAdvanceX(handle.GlyphSlot) + boldStrength / 64f;

            previousGlyph = glyphIndex;
            hasPrevious = true;
        }

        return width;
    }

    /// <summary>
    /// Rasterizes a single Unicode codepoint at this font's pixel size and style to an 8-bit coverage bitmap.
    /// Intended to be called by GlyphAtlas on a cache miss; callers that just need sizing should use
    /// MeasureString instead, which never rasterizes.
    /// </summary>
    /// <param name="codepoint">The Unicode codepoint to rasterize.</param>
    /// <param name="style">The font style to use when rasterizing the codepoint.</param>
    /// <returns>A RasterizedGlyph containing the bitmap and metrics of the rasterized codepoint.</returns>
    internal RasterizedGlyph RasterizeCodepoint(int codepoint, FontStyle style)
    {
        (FaceHandle handle, bool syntheticBold, bool syntheticItalic) = ResolveFace(style);
        uint glyphIndex = FT.FT_Get_Char_Index(handle.Face, (nuint)codepoint);

        LoadGlyph(handle, glyphIndex, syntheticBold, syntheticItalic, out long boldStrength);
        FT.FT_Render_Glyph(handle.GlyphSlot, FT.FT_RENDER_MODE_NORMAL);

        (int width, int height, int pitch, IntPtr buffer) = FT.GetBitmap(handle.GlyphSlot);
        byte[] coverage = new byte[width * height];
        if (width > 0 && height > 0 && buffer != IntPtr.Zero)
        {
            byte* src = (byte*)buffer;
            for (int row = 0; row < height; row++)
            {
                // A negative pitch means FreeType stored the bitmap bottom-up; normalize to top-down here.
                byte* srcRow = pitch >= 0 ? src + row * pitch : src + (height - 1 - row) * -pitch;
                for (int col = 0; col < width; col++)
                    coverage[row * width + col] = srcRow[col];
            }
        }

        return new RasterizedGlyph
        {
            Width = width,
            Height = height,
            BearingX = FT.GetBitmapLeft(handle.GlyphSlot),
            BearingY = FT.GetBitmapTop(handle.GlyphSlot),
            Advance = FT.GetAdvanceX(handle.GlyphSlot) + boldStrength / 64f,
            Coverage = coverage
        };
    }

    /// <summary>
    /// Returns this codepoint's outline as flattened contours in font pixels (Y up, origin on the baseline),
    /// cached per (codepoint, style). Intended for building extruded 3D text; callers must treat the returned
    /// contours as read-only. Synthetic bold and italic are baked in exactly as they are for rasterization.
    /// </summary>
    /// <param name="codepoint">The Unicode codepoint of the glyph to retrieve the outline for.</param>
    /// <param name="style">The font style to use when retrieving the glyph outline.</param>
    /// <returns>A GlyphOutline representing the flattened contours and advance of the glyph.</returns>
    internal GlyphOutline GetGlyphOutline(int codepoint, FontStyle style)
    {
        if (_outlineCache.TryGetValue((codepoint, style), out GlyphOutline cached)) return cached;

        (FaceHandle handle, bool syntheticBold, bool syntheticItalic) = ResolveFace(style);
        uint glyphIndex = FT.FT_Get_Char_Index(handle.Face, (nuint)codepoint);

        LoadGlyph(handle, glyphIndex, syntheticBold, syntheticItalic, out long boldStrength);

        GlyphOutline outline = new GlyphOutline(
            OutlineDecomposer.Decompose(FT.GetOutlinePtr(handle.GlyphSlot), OutlineFlatness),
            FT.GetAdvanceX(handle.GlyphSlot) + boldStrength / 64f);
        _outlineCache[(codepoint, style)] = outline;
        return outline;
    }

    ~Font()
    {
        if (!Disposed) Console.WriteLine("Warning: Font was not disposed before being finalized. This may cause a native resource leak.");
    }

    /// <summary>
    /// Disposes the font and releases all associated native resources. After calling this method, the font should not be used again.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        _cache.Remove(_key);
        FT.FT_Done_Face(_regular.Face);
        if (_bold is { } b) FT.FT_Done_Face(b.Face);
        if (_italic is { } i) FT.FT_Done_Face(i.Face);
        if (_boldItalic is { } bi) FT.FT_Done_Face(bi.Face);
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}
