using System;
using System.Text;

namespace odl3d;

/// <summary>
/// Text drawn from a composed bitmap: glyphs are rasterized once into a shared GlyphAtlas (see GlyphAtlas for
/// the caching strategy) and blitted into a single Texture sized to the text block, which subclasses then map
/// onto a quad. Used by Text2D for screen-space UI text and TextBillboard for camera-facing world-space text;
/// Text3D does not go through this path at all.
/// </summary>
public abstract class RasterizedText : Text
{
    private readonly GlyphAtlas _atlas;

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
    protected RasterizedText(Scene<Object> scene, Font font, string content, FontStyle style, TextAlign align, GlyphAtlas? atlas)
        : base(scene, font, content, style, align)
    {
        _atlas = atlas ?? GlyphAtlas.Shared;
    }

    /// <summary>The width of the composed texture in pixels, or 0 before the first build.</summary>
    public int PixelWidth => Texture?.Width ?? 0;

    /// <summary>The height of the composed texture in pixels, or 0 before the first build.</summary>
    public int PixelHeight => Texture?.Height ?? 0;

    /// <inheritdoc/>
    protected override void Rebuild()
    {
        Texture? old = Texture;

        string[] lines = Font.SplitLines(Content);
        float[] lineWidths = new float[lines.Length];
        float widest = 0f;
        for (int i = 0; i < lines.Length; i++)
        {
            lineWidths[i] = Font.MeasureLine(lines[i], Style);
            widest = MathF.Max(widest, lineWidths[i]);
        }

        int width = Math.Max(1, (int)MathF.Ceiling(widest));
        int height = Math.Max(1, (int)MathF.Ceiling(Font.LineHeight * lines.Length));
        int barThickness = Math.Max(1, (int)MathF.Ceiling(Font.UnderlineThickness));

        Texture composed = new Texture(width, height);

        for (int i = 0; i < lines.Length; i++)
        {
            int baselineY = (int)MathF.Round(Font.Ascender + Font.LineHeight * i);
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
                if (previousCodepoint >= 0) penX += Font.GetKerning(previousCodepoint, rune.Value, Style);

                AtlasGlyph glyph = _atlas.GetOrAdd(Font, Style, rune.Value);
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
                FillBar(composed, (int)MathF.Round(baselineY + Font.UnderlinePosition), barThickness, barX0, barX1);
            if (Strikethrough)
                FillBar(composed, (int)MathF.Round(baselineY - Font.Ascender * 0.35f), barThickness, barX0, barX1);
        }

        composed.FilterMode = TextureFilter.Linear;

        Texture = composed;
        old?.Dispose();
        OnTextureRebuilt();
    }

    /// <summary>Called after the composed Texture has been replaced, so subclasses can resize their quad.</summary>
    protected virtual void OnTextureRebuilt() { }

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
                int srcOffset = ((glyph.Y + row) * atlasTexture.Width + (glyph.X + col)) * 4;
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
