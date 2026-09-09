using System;
using decodl;

namespace odl3d;

/// <summary>
/// A single concept for both CPU-side pixel data and its uploaded GL handle;
/// there is no separate "bitmap" type — patterns are drawn directly into a Texture.
/// </summary>
public class Texture : IDisposable
{
    /// <summary>
    /// Width of the texture in pixels; the pixel buffer is Width * Height * 4 bytes (RGBA).
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Height of the texture in pixels; the pixel buffer is Width * Height * 4 bytes (RGBA).
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// The pixel buffer, in RGBA order, 4 bytes per pixel, row-major order, top-to-bottom.
    /// </summary>
    public byte[] Pixels { get; private set; }

    /// <summary>
    /// The GL handle of the texture; 0 if not yet uploaded. Upload() must be called to create the GL texture and copy the pixel data to it.
    /// </summary>
    public uint Handle { get; private set; }

    /// <summary>
    /// True if the pixel data has been uploaded to GL; false if Upload() has not yet been called or if the texture has been modified since the last upload.
    /// </summary>
    public bool Uploaded { get; private set; } = false;

    /// <summary>
    /// Horizontal wrap mode of the texture. Determines how the texture is sampled when texture coordinates are outside the [0, 1] range.
    /// </summary>
    public TextureWrap WrapModeH = TextureWrap.Repeat;

    /// <summary>
    /// Vertical wrap mode of the texture. Determines how the texture is sampled when texture coordinates are outside the [0, 1] range.
    /// </summary>
    public TextureWrap WrapModeV = TextureWrap.Repeat;

    /// <summary>
    /// Indicates whether this texture has been disposed and its resources released. After disposing, the texture should not be used again.
    /// </summary>
    public bool Disposed { get; private set; } = false;

    /// <summary>
    /// Invoked when this texture is disposed. Subscribers can use this event to perform cleanup or other actions when the texture is no longer needed.
    /// </summary>
    public event Action? OnDisposed;

    /// <summary>
    /// Creates a new Texture with the given width and height, allocating a pixel buffer of Width * Height * 4 bytes (RGBA). The pixel data is uninitialized; it must be filled in before calling Upload().
    /// </summary>
    /// <param name="width">Width of the texture in pixels.</param>
    /// <param name="height">Height of the texture in pixels.</param>
    public Texture(int width, int height)
    {
        Width = width;
        Height = height;
        Pixels = new byte[width * height * 4];
    }

    /// <summary>
    /// Loads a texture from a PNG file. The pixel buffer is filled with the decoded image data in RGBA order.
    /// </summary>
    /// <param name="filename">Path to the PNG file to load.</param>
    public Texture(string filename)
    {
        (byte[] bytes, int width, int height) = PNGDecoder.Decode(filename);
        Width = width;
        Height = height;
        Pixels = bytes;
    }

    ~Texture()
    {
        if (!Disposed) Console.WriteLine("Warning: Texture was not disposed before being finalized. This may cause a GL resource leak.");
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
    public static Texture FromColor(int width, int height, byte r, byte g, byte b, byte a = 255)
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
        return texture;
    }

    /// <summary>
    /// Creates a new Texture with the given width and height, filling the pixel buffer with the given color. The pixel data is initialized to the specified RGBA color.
    /// </summary>
    /// <param name="width">Width of the texture in pixels.</param>
    /// <param name="height">Height of the texture in pixels.</param>
    /// <param name="color">Color to fill the texture with (RGBA).</param>
    /// <returns>A new Texture initialized with the specified color.</returns>
    public static Texture FromColor(int width, int height, Color color)
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
    public static Texture FromCheckerboard(int width, int height, (byte R, byte G, byte B) colorA, (byte R, byte G, byte B) colorB, int cellSize = 8)
    {
        Texture texture = new Texture(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isA = ((x / cellSize) + (y / cellSize)) % 2 == 0;
                (byte R, byte G, byte B) color = isA ? colorA : colorB;
                int o = (y * width + x) * 4;
                texture.Pixels[o] = color.R;
                texture.Pixels[o + 1] = color.G;
                texture.Pixels[o + 2] = color.B;
                texture.Pixels[o + 3] = 255;
            }
        }
        return texture;
    }

    public static Texture FromGradient(int x, int y, int width, int height, Color c1, Color c2, Color c3, Color c4)
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
                int o = (dy * width + dx) * 4;
                texture.Pixels[o    ] = (byte) Math.Round(f1 * c1.R + f2 * c2.R + f3 * c3.R + f4 * c4.R);
                texture.Pixels[o + 1] = (byte) Math.Round(f1 * c1.G + f2 * c2.G + f3 * c3.G + f4 * c4.G);
                texture.Pixels[o + 2] = (byte) Math.Round(f1 * c1.B + f2 * c2.B + f3 * c3.B + f4 * c4.B);
                texture.Pixels[o + 3] = (byte) Math.Round(f1 * c1.A + f2 * c2.A + f3 * c3.A + f4 * c4.A);
            }
        }
        return texture;
    }

    /// <summary>
    /// Sets the pixel at the specified (x, y) coordinates to the given RGBA color. The pixel data is modified in the CPU-side buffer; Upload() must be called to update the GL texture with the new pixel data.
    /// </summary>
    /// <param name="x">X-coordinate of the pixel.</param>
    /// <param name="y">Y-coordinate of the pixel.</param>
    /// <param name="r">Red component of the color (0-255).</param>
    /// <param name="g">Green component of the color (0-255).</param>
    /// <param name="b">Blue component of the color (0-255).</param>
    /// <param name="a">Alpha component of the color (0-255).</param>
    public void SetPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
    {
        int o = (y * Width + x) * 4;
        Pixels[o] = r;
        Pixels[o + 1] = g;
        Pixels[o + 2] = b;
        Pixels[o + 3] = a;
        Uploaded = false;
    }

    /// <summary>
    /// Uploads the pixel data to the GPU, creating a GL texture if necessary. If the texture has already been uploaded and has not been modified since the last upload, this method does nothing. After calling this method, the texture can be bound and used for rendering.
    /// </summary>
    public void Upload()
    {
        if (Handle == 0)
        {
            GL.glGenTextures(1, out uint handle);
            Handle = handle;
        }

        GL.glBindTexture(GL.GL_TEXTURE_2D, Handle);
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (int)GL.GL_NEAREST);
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (int)GL.GL_NEAREST);
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, (int) WrapModeH);
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, (int) WrapModeV);
        GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, (int)GL.GL_RGBA, Width, Height, 0, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, Pixels);
        Uploaded = true;
    }

    /// <summary>
    /// Binds the texture to the specified texture unit for use in rendering. If the texture has not yet been uploaded, Upload() is called automatically. After calling this method, the texture is active and can be used in shaders.
    /// </summary>
    /// <param name="unit">Texture unit to bind the texture to (e.g., 0 for GL_TEXTURE0).</param>
    public void Bind(uint unit = 0)
    {
        if (!Uploaded) Upload();
        GL.glActiveTexture(GL.GL_TEXTURE0 + unit);
        GL.glBindTexture(GL.GL_TEXTURE_2D, Handle);
    }

    /// <summary>
    /// Disposes of the texture, releasing its GL handle and pixel buffer. After calling this method, the texture should not be used again. If the texture has already been disposed, this method does nothing.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        if (Handle != 0)
        {
            uint handle = Handle;
            GL.glDeleteTextures(1, ref handle);
        }
        Disposed = true;
        OnDisposed?.Invoke();
    }
}
