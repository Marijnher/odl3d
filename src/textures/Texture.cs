using System;
using decodl;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// Represents a texture that contains both CPU-side pixel data and its corresponding renderer handle.
/// </summary>
public class Texture : IDisposable
{
    /// <summary>
    /// Represents a texture that contains both CPU-side pixel data and its corresponding renderer handle.
    /// </summary>
    internal IRenderDevice Renderer => Window.Renderer;

    /// <summary>
    /// The renderer handle of the texture. This is the actual GPU resource that corresponds to the texture's pixel data.
    /// </summary>
    internal ITexture RenderTexture;
    
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
    public byte[] Pixels { get; internal set; }

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
    /// <param name="initialPixels">Optional initial pixel data in RGBA order. If not provided, a new buffer of the appropriate size will be allocated.</param>
    public Texture(uint width, uint height, byte[]? initialPixels = null)
    {
        initialPixels ??= new byte[width * height * 4];

        if (width < 1 || height < 1)
            throw new TextureException("Width and height must be greater than 0.");
        if (width * height * 4 != initialPixels.Length)
            throw new TextureException("Initial pixel array length does not match texture dimensions.");

        Width = width;
        Height = height;
        Pixels = initialPixels;
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
        byte[] bytes;
        int width;
        int height;
        try
        {
            (bytes, width, height) = PNGDecoder.Decode(filename);
        }
        catch (Exception ex)
        {
            throw new TextureException($"Failed to load texture from file '{filename}': {ex.Message}");
        }
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
    /// Marks the texture as invalid, indicating that its current pixel data may no longer match the renderer texture. This will force a re-upload on the next call to Upload().
    /// </summary>
    public void Invalidate()
    {
        Uploaded = false;
        hasPartialAlpha = null;
    }

    /// <summary>
    /// Uploads the current contents of the Pixels buffer to the renderer texture. This must be called after modifying Pixels (directly, via SetPixel, or via any of the factory methods) for the change to become visible when the texture is drawn; until then, the renderer texture retains whatever was last uploaded.
    /// </summary>
    public void Upload()
    {
        if (Disposed) throw new TextureException("Cannot upload a disposed texture.");
        RenderTexture.Upload(Pixels);
        Uploaded = true;
    }

    ~Texture()
    {
        if (!Disposed) Console.WriteLine("Warning: Texture was not disposed before being finalized. This may cause a renderer resource leak.");
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
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new TextureException($"Pixel coordinates ({x}, {y}) are out of bounds for texture of size {Width}x{Height}.");
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
