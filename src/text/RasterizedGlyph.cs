namespace odl3d;

/// <summary>
/// A single glyph rasterized to an 8-bit coverage bitmap by FreeType, along with the metrics needed to
/// position it relative to the pen and advance to the next glyph.
/// </summary>
internal struct RasterizedGlyph
{
    /// <summary>
    /// Width of the coverage bitmap in pixels; 0 for glyphs with no ink (e.g. space).
    /// </summary>
    public int Width;

    /// <summary>
    /// Height of the coverage bitmap in pixels; 0 for glyphs with no ink (e.g. space).
    /// </summary>
    public int Height;

    /// <summary>
    /// 8-bit alpha coverage, row-major, top-to-bottom, Width * Height bytes. Empty for glyphs with no ink.
    /// </summary>
    public byte[] Coverage;

    /// <summary>
    /// Horizontal offset from the pen position to the left edge of the bitmap.
    /// </summary>
    public int BearingX;

    /// <summary>
    /// Vertical offset from the baseline to the top edge of the bitmap.
    /// </summary>
    public int BearingY;

    /// <summary>
    /// Horizontal distance to advance the pen after drawing this glyph, in pixels.
    /// </summary>
    public float Advance;
}
