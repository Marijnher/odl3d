namespace odl3d;

/// <summary>
/// Where a single glyph's coverage bitmap lives within a GlyphAtlas's backing Texture, plus the metrics needed
/// to position it relative to the pen and advance to the next glyph. Width/Height are 0 for glyphs with no ink
/// (e.g. space), in which case X/Y are meaningless and only Advance matters.
/// </summary>
internal struct AtlasGlyph
{
    public int X;
    public int Y;
    public int Width;
    public int Height;
    public int BearingX;
    public int BearingY;
    public float Advance;
}
