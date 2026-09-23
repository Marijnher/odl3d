using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL texture, encapsulating its handle, dimensions, format, usage, and related information.
/// </summary>
internal class OpenGLTexture : ITexture
{
    /// <summary>
    /// Gets the handle of the OpenGL texture.
    /// </summary>
    public uint Handle { get; }

    /// <summary>
    /// Gets the width of the texture in pixels.
    /// </summary>
    public uint Width { get; }

    /// <summary>
    /// Gets the height of the texture in pixels.
    /// </summary>
    public uint Height { get; }

    /// <summary>
    /// Gets the depth of the texture in pixels.
    /// </summary>
    public int Depth { get; }

    /// <summary>
    /// Gets the format of the texture.
    /// </summary>
    public TextureFormat Format { get; }

    /// <summary>
    /// Gets the usage flags of the texture.
    /// </summary>
    public TextureUsage Usage { get; }

    /// <summary>
    /// Gets the number of samples per texel for multisampled textures.
    /// </summary>
    public int SampleCount { get; }

    /// <summary>
    /// Gets a value indicating whether the texture has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the texture has valid mipmaps.
    /// </summary>
    private bool HasValidMipmaps;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLTexture"/> class with the specified description and optional initial data.
    /// </summary>
    /// <param name="description">The description of the texture to create.</param>
    /// <param name="initialData">Optional initial data to upload to the texture.</param>
    public OpenGLTexture(TextureDescription description, byte[]? initialData = null)
    {
        Width = description.Width;
        Height = description.Height;
        Depth = description.Depth;
        Format = description.Format;
        Usage = description.Usage;
        SampleCount = description.SampleCount;
        GL.glGenTextures(1, out uint handle);
        Handle = handle;
        if (initialData != null) Upload(initialData);
    }

    /// <summary>
    /// Uploads the given data to the GPU for this texture.
    /// </summary>
    /// <param name="data">The data to upload.</param>
    /// <param name="mipLevel">The mip level to upload the data to.</param>
    public void Upload(byte[] data, int mipLevel = 0)
    {
        GL.glBindTexture(GL.GL_TEXTURE_2D, Handle);
        GL.glTexImage2D(
            target: GL.GL_TEXTURE_2D,
            level: mipLevel,
            internalFormat: GetGLInternalFormat(Format),
            width: (int) Width,
            height: (int) Height,
            border: 0,
            format: GetGLPixelFormat(Format),
            type: GetGLPixelType(Format),
            pixels: data
        );
        GL.glBindTexture(GL.GL_TEXTURE_2D, 0);
        HasValidMipmaps = false;
    }

    /// <summary>
    /// Validates the mipmaps for the texture, generating them if they are not already valid.
    /// </summary>
    public void ValidateMipmaps()
    {
        if (HasValidMipmaps) return;
        GL.glBindTexture(GL.GL_TEXTURE_2D, Handle);
        GL.glGenerateMipmap(GL.GL_TEXTURE_2D);
        GL.glBindTexture(GL.GL_TEXTURE_2D, 0);
        HasValidMipmaps = true;
    }

    /// <summary>
    /// Gets the internal format for how the data is stored in the GPU.
    /// </summary>
    /// <param name="f">The texture format.</param>
    /// <returns>The corresponding OpenGL internal format.</returns>
    /// <exception cref="RenderException">Thrown when the texture format is not supported.</exception>
    private static uint GetGLInternalFormat(TextureFormat f) => f switch
    {
        TextureFormat.RGBA8Unorm => GL.GL_RGBA8,
        TextureFormat.BGRA8Unorm => GL.GL_RGBA8,
        TextureFormat.RGBA16Float => GL.GL_RGBA16F,
        TextureFormat.RGBA32Float => GL.GL_RGBA32F,
        TextureFormat.Depth16Unorm => GL.GL_DEPTH_COMPONENT16,
        TextureFormat.Depth24UnormStencil8 => GL.GL_DEPTH24_STENCIL8,
        TextureFormat.Depth32Float => GL.GL_DEPTH_COMPONENT32F,
        _ => throw new RenderException($"Unsupported texture format: {f}")
    };

    /// <summary>
    /// Gets the pixel format for how the data is represented when uploaded to the GPU.
    /// </summary>
    /// <param name="f">The texture format.</param>
    /// <returns>The corresponding OpenGL pixel format.</returns>
    /// <exception cref="NotSupportedException">Thrown when the texture format is not supported.</exception>
    private static uint GetGLPixelFormat(TextureFormat f) => f switch
    {
        TextureFormat.RGBA8Unorm => GL.GL_RGBA,
        TextureFormat.BGRA8Unorm => GL.GL_BGRA,
        TextureFormat.RGBA16Float => GL.GL_RGBA,
        TextureFormat.RGBA32Float => GL.GL_RGBA,
        TextureFormat.Depth16Unorm => GL.GL_DEPTH_COMPONENT16,
        TextureFormat.Depth24UnormStencil8 => GL.GL_DEPTH24_STENCIL8,
        TextureFormat.Depth32Float => GL.GL_DEPTH_COMPONENT32F,
        _ => throw new NotSupportedException(f.ToString())
    };

    /// <summary>
    /// Gets the pixel type for how the data is represented when uploaded to the GPU.
    /// </summary>
    /// <param name="f">The texture format.</param>
    /// <returns>The corresponding OpenGL pixel type.</returns>
    /// <exception cref="NotSupportedException">Thrown when the texture format is not supported.</exception>
    private static uint GetGLPixelType(TextureFormat f) => f switch
    {
        TextureFormat.RGBA8Unorm => GL.GL_UNSIGNED_BYTE,
        TextureFormat.BGRA8Unorm => GL.GL_UNSIGNED_BYTE,
        TextureFormat.RGBA16Float => GL.GL_HALF_FLOAT,
        TextureFormat.RGBA32Float => GL.GL_FLOAT,
        TextureFormat.Depth16Unorm => GL.GL_UNSIGNED_SHORT,
        TextureFormat.Depth24UnormStencil8 => GL.GL_UNSIGNED_INT_24_8,
        TextureFormat.Depth32Float => GL.GL_FLOAT,
        _ => throw new NotSupportedException(f.ToString())
    };

    /// <summary>
    /// Disposes of the OpenGL texture, releasing its resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        uint handle = Handle;
        GL.glDeleteTextures(1, ref handle);
        Disposed = true;
    }
}