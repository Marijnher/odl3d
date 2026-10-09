using System;
using System.Text;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// Text drawn from a composed bitmap: glyphs are rasterized once into a shared GlyphAtlas (see GlyphAtlas for
/// the caching strategy) and blitted into a single Texture sized to the text block, which subclasses then map
/// onto a quad. Used by Text2D for screen-space UI text and TextBillboard for camera-facing world-space text;
/// Text3D does not go through this path at all.
/// </summary>
public abstract class RasterizedText : Text
{
    /// <summary>
    /// The width of the composed texture in pixels, or 0 before the first build.
    /// </summary>
    public uint PixelWidth => Texture?.Width ?? 0;

    /// <summary>
    /// The height of the composed texture in pixels, or 0 before the first build.
    /// </summary>
    public uint PixelHeight => Texture?.Height ?? 0;

    private readonly GlyphAtlas _atlas;

    private float _rasterScale = 1f;
    /// <summary>
    /// Ratio of rasterized glyph pixels to the font's nominal pixel size. Raised on high-DPI displays so the
    /// composed texture maps 1:1 onto physical pixels instead of being upscaled. Rebuilds when changed.
    /// </summary>
    protected float RasterScale
    {
        get => _rasterScale;
        set { if (_rasterScale == value) return; _rasterScale = value; Rebuild(); }
    }

    /// <summary>
    /// Creates a new bitmap-composed text drawable, using the shared process-wide glyph atlas unless a custom
    /// one is supplied.
    /// </summary>
    /// <param name="scene">The scene to which this text belongs.</param>
    /// <param name="font">The font to measure and rasterize this text's glyphs with.</param>
    /// <param name="content">The initial text to display. May contain line breaks.</param>
    /// <param name="style">The initial Bold/Italic style.</param>
    /// <param name="align">Horizontal alignment of each line within the text block.</param>
    /// <param name="atlas">An optional custom GlyphAtlas; defaults to the shared process-wide atlas.</param>
    protected RasterizedText(Font font, FontStyle style, TextAlign align, GlyphAtlas? atlas)
        : base(font, style, align)
    {
        _atlas = atlas ?? GlyphAtlas.Shared;
        // Rasterized text is flat artwork, so scene lights do not shade it unless asked to.
        Lit = false;
    }

    /// <summary>
    /// Rebuilds the composed texture by rasterizing all glyphs and arranging them according to the current text content, style, and alignment.
    /// </summary>
    protected override void Rebuild()
    {
        Texture? old = Texture;
        Font font = RasterScale == 1f ? Font : Font.WithPixelSize(Math.Max(1, (int) MathF.Round(Font.PixelSize * RasterScale)));

        string[] lines = Font.SplitLines(Content);
        float[] lineWidths = new float[lines.Length];
        float widest = 0f;
        for (int i = 0; i < lines.Length; i++)
        {
            lineWidths[i] = font.MeasureLine(lines[i], Style);
            widest = MathF.Max(widest, lineWidths[i]);
        }

        uint width = Math.Max(1, (uint)MathF.Ceiling(widest));
        uint height = Math.Max(1, (uint)MathF.Ceiling(font.LineHeight * lines.Length));
        int barThickness = Math.Max(1, (int)MathF.Ceiling(font.UnderlineThickness));

        Texture composed = new Texture(width, height);

        for (int i = 0; i < lines.Length; i++)
        {
            int baselineY = (int)MathF.Round(font.Ascender + font.LineHeight * i);
            float startX = Align switch
            {
                TextAlign.Center => (width - lineWidths[i]) / 2f,
                TextAlign.Right => width - lineWidths[i],
                _ => 0f
            };

            float penX = startX;
            int previousCodepoint = -1;
            foreach (Rune rune in lines[i].EnumerateRunes())
            {
                if (previousCodepoint >= 0) penX += font.GetKerning(previousCodepoint, rune.Value, Style);

                AtlasGlyph glyph = _atlas.GetOrAdd(font, Style, rune.Value);
                if (glyph.Width > 0 && glyph.Height > 0)
                {
                    int destX = (int)MathF.Round(penX) + glyph.BearingX;
                    int destY = baselineY - glyph.BearingY;
                    BlitGlyph(composed, glyph, destX, destY);
                }

                penX += glyph.Advance;
                previousCodepoint = rune.Value;
            }

            int barX0 = (int)MathF.Round(startX);
            int barX1 = (int)MathF.Round(penX);
            if (Underline)
                FillBar(composed, (int)MathF.Round(baselineY + font.UnderlinePosition), barThickness, barX0, barX1);
            if (Strikethrough)
                FillBar(composed, (int)MathF.Round(baselineY - font.Ascender * 0.35f), barThickness, barX0, barX1);
        }

        Sampler.MinFilter = TextureFilter.Linear;
        Sampler.MagFilter = TextureFilter.Linear;

        Texture = composed;
        old?.Dispose();
    }

    private void BlitGlyph(Texture dest, AtlasGlyph glyph, int destX, int destY)
    {
        Texture atlasTexture = _atlas.Texture;
        for (int row = 0; row < glyph.Height; row++)
        {
            int dy = destY + row;
            if (dy < 0 || dy >= dest.Height) continue;
            for (int col = 0; col < glyph.Width; col++)
            {
                int dx = destX + col;
                if (dx < 0 || dx >= dest.Width) continue;
                int srcOffset = (int) ((glyph.Y + row) * atlasTexture.Width + (glyph.X + col)) * 4;
                byte alpha = atlasTexture.Pixels[srcOffset + 3];
                if (alpha == 0) continue;
                dest.SetPixel(dx, dy, 255, 255, 255, alpha);
            }
        }
    }

    private static void FillBar(Texture texture, int y0, int thickness, int x0, int x1)
    {
        for (int dy = y0; dy < y0 + thickness; dy++)
        {
            if (dy < 0 || dy >= texture.Height) continue;
            for (int dx = Math.Max(0, x0); dx < Math.Min(texture.Width, x1); dx++)
                texture.SetPixel(dx, dy, 255, 255, 255, 255);
        }
    }
}
