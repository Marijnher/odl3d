using System;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLTexture : ITexture
{
    public uint Handle { get; }

    public uint Width { get; }
    public uint Height { get; }
    public int Depth { get; }

    public TextureFormat Format { get; }
    public TextureUsage Usage { get; }

    public int SampleCount { get; }

    public bool Disposed { get; private set; }
    
    private bool HasValidMipmaps;

    /// <summary>
    /// Creates a new OpenGL texture with the specified description and optional initial data.
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

    public void Dispose()
    {
        if (Disposed) return;
        uint handle = Handle;
        GL.glDeleteTextures(1, ref handle);
        Disposed = true;
    }
}