using System;
using System.Collections.Generic;

namespace odl3d;

/// <summary>
/// Caches rasterized glyphs so each distinct (font, style, codepoint) combination is only ever rasterized by
/// FreeType once, and packs them into a single shared RGBA Texture (white RGB, coverage in alpha) using a
/// simple growable shelf packer. A Text composes its own displayed Texture by copying pixels out of this
/// atlas's CPU-side buffer, so the atlas itself never needs to be uploaded to the GPU for that path.
/// </summary>
/// <remarks>
/// //TODO: Tier 2 - a batched drawable that samples this atlas directly on the GPU (one quad per glyph,
/// UVs into this texture) instead of every Text re-composing its own texture, to avoid per-string CPU
/// compositing for large volumes of frequently-changing text. Will need this atlas's Texture uploaded/rebound
/// (and re-uploaded whenever Grow() reallocates it) instead of only being read from on the CPU as it is now.
/// </remarks>
public sealed class GlyphAtlas
{
    /// <summary>Process-wide default atlas, shared by all Text instances that don't request their own.</summary>
    public static GlyphAtlas Shared => _shared ??= new GlyphAtlas();
    private static GlyphAtlas? _shared;

    private const int Padding = 1;

    private readonly Dictionary<(Font Font, FontStyle Style, int Codepoint), AtlasGlyph> _cache = new();
    private readonly List<(int Y, int Height, int NextX)> _shelves = new();
    private int _bottom;

    /// <summary>
    /// The shared CPU-side atlas buffer that glyph coverage bitmaps are packed into.
    /// </summary>
    public Texture Texture { get; private set; }

    /// <summary>
    /// Initializes a new instance of the GlyphAtlas class with the specified initial size.
    /// </summary>
    /// <param name="initialSize">The initial size of the atlas texture in pixels.</param>
    public GlyphAtlas(uint initialSize = 512)
    {
        Texture = new Texture(initialSize, initialSize);
    }

    /// <summary>
    /// Returns the atlas placement + metrics for the given glyph, rasterizing and packing it on first use and
    /// returning the cached placement on every subsequent call.
    /// </summary>
    /// <param name="font">The font to retrieve the glyph from.</param>
    /// <param name="style">The style of the font.</param>
    /// <param name="codepoint">The Unicode codepoint of the glyph.</param>
    /// <returns>The atlas placement and metrics for the specified glyph.</returns>
    internal AtlasGlyph GetOrAdd(Font font, FontStyle style, int codepoint)
    {
        (Font font, FontStyle style, int codepoint) key = (font, style, codepoint);
        if (_cache.TryGetValue(key, out AtlasGlyph existing)) return existing;

        RasterizedGlyph glyph = font.RasterizeCodepoint(codepoint, style);
        AtlasGlyph placed = Place(glyph);
        _cache[key] = placed;
        return placed;
    }

    private AtlasGlyph Place(RasterizedGlyph glyph)
    {
        if (glyph.Width == 0 || glyph.Height == 0)
            return new AtlasGlyph { BearingX = glyph.BearingX, BearingY = glyph.BearingY, Advance = glyph.Advance };

        int neededWidth = glyph.Width + Padding;
        int neededHeight = glyph.Height + Padding;

        for (int i = 0; i < _shelves.Count; i++)
        {
            (int Y, int Height, int NextX) shelf = _shelves[i];
            if (shelf.Height >= neededHeight && Texture.Width - shelf.NextX >= neededWidth)
            {
                Blit(glyph, shelf.NextX, shelf.Y);
                AtlasGlyph placed = new AtlasGlyph
                {
                    X = shelf.NextX, Y = shelf.Y, Width = glyph.Width, Height = glyph.Height,
                    BearingX = glyph.BearingX, BearingY = glyph.BearingY, Advance = glyph.Advance
                };
                _shelves[i] = (shelf.Y, shelf.Height, shelf.NextX + neededWidth);
                return placed;
            }
        }

        while (_bottom + neededHeight > Texture.Height || neededWidth > Texture.Width)
            Grow();

        int shelfX = 0;
        int shelfY = _bottom;
        Blit(glyph, shelfX, shelfY);
        _shelves.Add((shelfY, neededHeight, shelfX + neededWidth));
        _bottom += neededHeight;

        return new AtlasGlyph
        {
            X = shelfX, Y = shelfY, Width = glyph.Width, Height = glyph.Height,
            BearingX = glyph.BearingX, BearingY = glyph.BearingY, Advance = glyph.Advance
        };
    }

    private void Blit(RasterizedGlyph glyph, int x, int y)
    {
        for (int row = 0; row < glyph.Height; row++)
        {
            for (int col = 0; col < glyph.Width; col++)
            {
                byte coverage = glyph.Coverage[row * glyph.Width + col];
                Texture.SetPixel(x + col, y + row, 255, 255, 255, coverage);
            }
        }
    }

    private void Grow()
    {
        Texture old = Texture;
        byte[] newPixels = new byte[old.Width * old.Height * 2 * 4];
        Array.Copy(old.Pixels, newPixels, old.Pixels.Length);
        Texture = new Texture(old.Width, old.Height * 2, newPixels);
        old.Dispose();
    }
}
