using System;
using decodl;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// A single concept for both CPU-side pixel data and its uploaded renderer handle;
/// there is no separate "bitmap" type — patterns are drawn directly into a Texture.
/// </summary>
public class Texture : IDisposable
{
    protected IRenderDevice Renderer => Window.Renderer;

    public ITexture RenderTexture;
    
    /// <summary>
    /// Width of the texture in pixels; the pixel buffer is Width * Height * 4 bytes (RGBA).
    /// </summary>
    public uint Width { get; private set; }

    /// <summary>
    /// Height of the texture in pixels; the pixel buffer is Width * Height * 4 bytes (RGBA).
    /// </summary>
    public uint Height { get; private set; }

    /// <summary>
    /// The pixel buffer, in RGBA order, 4 bytes per pixel, row-major order, top-to-bottom.
    /// </summary>
    public byte[] Pixels { get; private set; }

    /// <summary>
    /// The renderer handle of the texture; 0 if not yet uploaded. Upload() must be called to create the renderer texture and copy the pixel data to it.
    /// </summary>
    public uint Handle { get; private set; }

    /// <summary>
    /// True if the pixel data has been uploaded to the active renderer; false if Upload() has not yet been called or if the texture has been modified since the last upload.
    /// </summary>
    public bool Uploaded { get; private set; } = false;

    /// <summary>
    /// Indicates whether this texture has been disposed and its resources released. After disposing, the texture should not be used again.
    /// </summary>
    public bool Disposed { get; private set; }

    private bool? hasPartialAlpha;

    /// <summary>
    /// True if any pixel in this texture has an alpha value other than 0 or 255 (i.e. genuine partial transparency, such as a soft shadow). Textures whose alpha is always fully opaque or fully transparent (hard cutouts) return false, since those are handled by discarding transparent fragments rather than blending. The full pixel buffer is scanned only once and cached; SetPixel updates the cached result directly instead of forcing a rescan, so per-pixel edits stay O(1).
    /// </summary>
    public bool HasPartialAlpha
    {
        get
        {
            if (hasPartialAlpha == null)
            {
                bool found = false;
                for (int i = 3; i < Pixels.Length; i += 4)
                {
                    byte a = Pixels[i];
                    if (a != 0 && a != 255) { found = true; break; }
                }
                hasPartialAlpha = found;
            }
            return hasPartialAlpha.Value;
        }
    }

    /// <summary>
    /// Invoked when this texture is disposed. Subscribers can use this event to perform cleanup or other actions when the texture is no longer needed.
    /// </summary>
    public event Action? OnDisposed;

    /// <summary>
    /// Creates a new Texture with the given width and height, allocating a pixel buffer of Width * Height * 4 bytes (RGBA). The pixel data is uninitialized; it must be filled in before calling Upload().
    /// </summary>
    /// <param name="width">Width of the texture in pixels.</param>
    /// <param name="height">Height of the texture in pixels.</param>
    public Texture(uint width, uint height)
    {
        Width = width;
        Height = height;
        Pixels = new byte[width * height * 4];
        RenderTexture = Renderer.CreateTexture(new TextureDescription
        {
            Format = TextureFormat.RGBA8Unorm,
            Width = Width,
            Height = Height
        }, Pixels);
        // The renderer texture above was just created from the current (initial) Pixels contents.
        Uploaded = true;
    }

    /// <summary>
    /// Loads a texture from a PNG file. The pixel buffer is filled with the decoded image data in RGBA order.
    /// </summary>
    /// <param name="filename">Path to the PNG file to load.</param>
    public Texture(string filename)
    {
        (byte[] bytes, int width, int height) = PNGDecoder.Decode(filename);
        Width = (uint) width;
        Height = (uint) height;
        Pixels = bytes;
        RenderTexture = Renderer.CreateTexture(new TextureDescription
        {
            Format = TextureFormat.RGBA8Unorm,
            Width = Width,
            Height = Height
        }, Pixels);
        // The renderer texture above was just created from the current (initial) Pixels contents.
        Uploaded = true;
    }

    /// <summary>
    /// Uploads the current contents of the Pixels buffer to the renderer texture. This must be called after modifying Pixels (directly, via SetPixel, or via any of the factory methods) for the change to become visible when the texture is drawn; until then, the renderer texture retains whatever was last uploaded.
    /// </summary>
    public void Upload()
    {
        if (Disposed) return;
        RenderTexture.Upload(Pixels);
        Uploaded = true;
    }

    ~Texture()
    {
        if (!Disposed) Console.WriteLine("Warning: Texture was not disposed before being finalized. This may cause a renderer resource leak.");
    }

    /// <summary>
    /// Creates a new Texture with the given width and height, filling the pixel buffer with the given color. The pixel data is initialized to the specified RGBA color.
    /// </summary>
    /// <param name="width">Width of the texture in pixels.</param>
    /// <param name="height">Height of the texture in pixels.</param>
    /// <param name="r">Red component of the color (0-255).</param>
    /// <param name="g">Green component of the color (0-255).</param>
    /// <param name="b">Blue component of the color (0-255).</param>
    /// <param name="a">Alpha component of the color (0-255).</param>
    /// <returns>A new Texture initialized with the specified color.</returns>
    public static Texture FromColor(uint width, uint height, byte r, byte g, byte b, byte a = 255)
    {
        Texture texture = new Texture(width, height);
        for (int i = 0; i < width * height; i++)
        {
            int o = i * 4;
            texture.Pixels[o] = r;
            texture.Pixels[o + 1] = g;
            texture.Pixels[o + 2] = b;
            texture.Pixels[o + 3] = a;
        }
        texture.Upload();
        return texture;
    }

    /// <summary>
    /// Creates a new Texture with the given width and height, filling the pixel buffer with the given color. The pixel data is initialized to the specified RGBA color.
    /// </summary>
    /// <param name="width">Width of the texture in pixels.</param>
    /// <param name="height">Height of the texture in pixels.</param>
    /// <param name="color">Color to fill the texture with (RGBA).</param>
    /// <returns>A new Texture initialized with the specified color.</returns>
    public static Texture FromColor(uint width, uint height, Color color)
    {
        return FromColor(width, height, color.R, color.G, color.B, color.A);
    }

    /// <summary>
    /// Creates a new Texture with the given width and height, filling the pixel buffer with a checkerboard pattern of the two specified colors. The pixel data is initialized to a checkerboard pattern of the specified colors, with each square being cellSize pixels wide and tall.
    /// </summary>
    /// <param name="width">Width of the texture in pixels.</param>
    /// <param name="height">Height of the texture in pixels.</param>
    /// <param name="colorA">First color of the checkerboard pattern (RGB).</param>
    /// <param name="colorB">Second color of the checkerboard pattern (RGB).</param>
    /// <param name="cellSize">Size of each square in the checkerboard pattern in pixels.</param>
    /// <returns>A new Texture initialized with the checkerboard pattern.</returns>
    public static Texture FromCheckerboard(uint width, uint height, (byte R, byte G, byte B) colorA, (byte R, byte G, byte B) colorB, int cellSize = 8)
    {
        Texture texture = new Texture(width, height);
        for (uint y = 0; y < height; y++)
        {
            for (uint x = 0; x < width; x++)
            {
                bool isA = ((x / cellSize) + (y / cellSize)) % 2 == 0;
                (byte R, byte G, byte B) color = isA ? colorA : colorB;
                uint o = (y * width + x) * 4;
                texture.Pixels[o] = color.R;
                texture.Pixels[o + 1] = color.G;
                texture.Pixels[o + 2] = color.B;
                texture.Pixels[o + 3] = 255;
            }
        }
        texture.Upload();
        return texture;
    }

    /// <summary>
    /// Creates a new Texture with a gradient defined by the four corner colors. The gradient is interpolated across the specified rectangular region.
    /// </summary>
    /// <param name="x">X-coordinate of the top-left corner of the gradient region.</param>
    /// <param name="y">Y-coordinate of the top-left corner of the gradient region.</param>
    /// <param name="width">Width of the gradient region in pixels.</param>
    /// <param name="height">Height of the gradient region in pixels.</param>
    /// <param name="c1">Color of the top-left corner.</param>
    /// <param name="c2">Color of the top-right corner.</param>
    /// <param name="c3">Color of the bottom-left corner.</param>
    /// <param name="c4">Color of the bottom-right corner.</param>
    /// <returns>A new Texture initialized with the specified gradient.</returns>
    public static Texture FromGradient(int x, int y, uint width, uint height, Color c1, Color c2, Color c3, Color c4)
    {
        Texture texture = new Texture(width, height);
        for (int dy = y; dy < y + height; dy++)
        {
            for (int dx = x; dx < x + width; dx++)
            {
                double xl = dx - x;
                double xr = x + width - 1 - dx;
                double yt = dy - y;
                double yb = y + height - 1 - dy;
                double fxr = (xl / (xl + xr));
                double fxl = 1 - fxr;
                double fyb = (yt / (yt + yb));
                double fyt = 1 - fyb;
                double f1 = fxl * fyt;
                double f2 = fxr * fyt;
                double f3 = fxl * fyb;
                double f4 = fxr * fyb;
                uint o = (uint) (dy * width + dx) * 4;
                texture.Pixels[o    ] = (byte) Math.Round(f1 * c1.R + f2 * c2.R + f3 * c3.R + f4 * c4.R);
                texture.Pixels[o + 1] = (byte) Math.Round(f1 * c1.G + f2 * c2.G + f3 * c3.G + f4 * c4.G);
                texture.Pixels[o + 2] = (byte) Math.Round(f1 * c1.B + f2 * c2.B + f3 * c3.B + f4 * c4.B);
                texture.Pixels[o + 3] = (byte) Math.Round(f1 * c1.A + f2 * c2.A + f3 * c3.A + f4 * c4.A);
            }
        }
        texture.Upload();
        return texture;
    }

    /// <summary>
    /// Sets the pixel at the specified (x, y) coordinates to the given RGBA color. The pixel data is modified in the CPU-side buffer; Upload() must be called to update the renderer texture with the new pixel data.
    /// </summary>
    /// <param name="x">X-coordinate of the pixel.</param>
    /// <param name="y">Y-coordinate of the pixel.</param>
    /// <param name="r">Red component of the color (0-255).</param>
    /// <param name="g">Green component of the color (0-255).</param>
    /// <param name="b">Blue component of the color (0-255).</param>
    /// <param name="a">Alpha component of the color (0-255).</param>
    public void SetPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
    {
        uint o = (uint) (y * Width + x) * 4;
        Pixels[o] = r;
        Pixels[o + 1] = g;
        Pixels[o + 2] = b;
        Pixels[o + 3] = a;
        Uploaded = false;
        // Only ever flips false->true here; a full rescan would be needed to detect the reverse, which isn't worth the cost.
        if (a != 0 && a != 255) hasPartialAlpha = true;
    }

    /// <summary>
    /// Disposes of the texture, releasing its renderer handle and pixel buffer. After calling this method, the texture should not be used again. If the texture has already been disposed, this method does nothing.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        RenderTexture.Dispose();
        Disposed = true;
        OnDisposed?.Invoke();
    }
}
