namespace odl3d;

/// <summary>
/// Where a single glyph's coverage bitmap lives within a GlyphAtlas's backing Texture, plus the metrics needed
/// to position it relative to the pen and advance to the next glyph. Width/Height are 0 for glyphs with no ink
/// (e.g. space), in which case X/Y are meaningless and only Advance matters.
/// </summary>
internal struct AtlasGlyph
{
    /// <summary>
    /// The X coordinate of the glyph's coverage bitmap within the atlas texture.
    /// </summary>
    public int X;

    /// <summary>
    /// The Y coordinate of the glyph's coverage bitmap within the atlas texture.
    /// </summary>
    public int Y;

    /// <summary>
    /// The width of the glyph's coverage bitmap.
    /// </summary>
    public int Width;

    /// <summary>
    /// The height of the glyph's coverage bitmap.
    /// </summary>
    public int Height;

    /// <summary>
    /// The horizontal distance from the pen position to the left edge of the glyph's coverage bitmap.
    /// </summary>
    public int BearingX;

    /// <summary>
    /// The vertical distance from the pen position to the top edge of the glyph's coverage bitmap.
    /// </summary>
    public int BearingY;
    
    /// <summary>
    /// The distance to advance the pen position after rendering this glyph.
    /// </summary>
    public float Advance;
}
