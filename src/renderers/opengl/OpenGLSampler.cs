using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL sampler object that encapsulates texture sampling state.
/// </summary>
internal class OpenGLSampler : ISampler
{
    /// <summary>
    /// Gets the handle to the OpenGL sampler object.
    /// </summary>
    public uint Handle { get; }

    /// <summary>
    /// Gets the texture minification filter of the sampler.
    /// </summary>
    public TextureFilter MinFilter { get; }

    /// <summary>
    /// Gets the texture magnification filter of the sampler.
    /// </summary>
    public TextureFilter MagFilter { get; }

    /// <summary>
    /// Gets the mipmap filter of the sampler.
    /// </summary>
    public MipmapFilter MipmapFilter { get; }

    /// <summary>
    /// Gets the texture wrap mode for the U coordinate of the sampler.
    /// </summary>
    public TextureWrap WrapU { get; }

    /// <summary>
    /// Gets the texture wrap mode for the V coordinate of the sampler.
    /// </summary>
    public TextureWrap WrapV { get; }

    /// <summary>
    /// Gets the texture wrap mode for the W coordinate of the sampler.
    /// </summary>
    public TextureWrap WrapW { get; }

    /// <summary>
    /// Gets the anisotropic filtering level of the sampler.
    /// </summary>
    public AnisotropicFilter Anisotropy { get; }

    /// <summary>
    /// Gets the comparison function of the sampler, if any.
    /// </summary>
    public CompareFunction? Comparison { get; }

    /// <summary>
    /// Gets a value indicating whether the sampler has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLSampler"/> class with the specified sampler description.
    /// </summary>
    /// <param name="description">The description of the sampler to create.</param>
    public OpenGLSampler(SamplerDescription description)
    {
        MinFilter = description.MinFilter;
        MagFilter = description.MagFilter;
        MipmapFilter = description.MipmapFilter;
        WrapU = description.WrapU;
        WrapV = description.WrapV;
        WrapW = description.WrapW;
        Anisotropy = description.Anisotropy;
        Comparison = description.Comparison;

        GL.glGenSamplers(1, out uint handle);
        Handle = handle;
        GL.glSamplerParameteri(Handle, GL.GL_TEXTURE_MIN_FILTER, GetGLMinFilter(MinFilter, MipmapFilter));
        GL.glSamplerParameteri(Handle, GL.GL_TEXTURE_MAG_FILTER, GetGLMagFilter(MagFilter));
        GL.glSamplerParameteri(Handle, GL.GL_TEXTURE_WRAP_S, GetGLWrap(WrapU));
        GL.glSamplerParameteri(Handle, GL.GL_TEXTURE_WRAP_T, GetGLWrap(WrapV));
        GL.glSamplerParameteri(Handle, GL.GL_TEXTURE_WRAP_R, GetGLWrap(WrapW));
        GL.glSamplerParameteri(Handle, GL.GL_TEXTURE_MAX_ANISOTROPY_EXT, (int) Anisotropy);
        GL.glSamplerParameteri(Handle, GL.GL_TEXTURE_COMPARE_FUNC, GetGLCompareFunction(Comparison));
    }

    private static int GetGLMinFilter(TextureFilter minFilter, MipmapFilter mipmapFilter) => (minFilter, mipmapFilter) switch
    {
        (TextureFilter.Nearest, MipmapFilter.Nearest) => GL.GL_NEAREST_MIPMAP_NEAREST,
        (TextureFilter.Nearest, MipmapFilter.Linear) => GL.GL_NEAREST_MIPMAP_LINEAR,
        (TextureFilter.Linear, MipmapFilter.Nearest) => GL.GL_LINEAR_MIPMAP_NEAREST,
        (TextureFilter.Linear, MipmapFilter.Linear) => GL.GL_LINEAR_MIPMAP_LINEAR,
        (TextureFilter.Nearest, MipmapFilter.None) => GL.GL_NEAREST,
        (TextureFilter.Linear, MipmapFilter.None) => GL.GL_LINEAR,
        _ => throw new RenderException($"Unsupported combination of min filter {minFilter} and mipmap filter {mipmapFilter}")
    };

    private static int GetGLMagFilter(TextureFilter magFilter) => magFilter switch
    {
        TextureFilter.Nearest => GL.GL_NEAREST,
        TextureFilter.Linear => GL.GL_LINEAR,
        _ => throw new RenderException($"Unsupported mag filter {magFilter}")
    };

    private static int GetGLWrap(TextureWrap wrap) => wrap switch
    {
        TextureWrap.Repeat => GL.GL_REPEAT,
        TextureWrap.Mirror => GL.GL_MIRRORED_REPEAT,
        TextureWrap.Clamp => GL.GL_CLAMP_TO_EDGE,
        _ => throw new RenderException($"Unsupported wrap mode {wrap}")
    };

    private static int GetGLCompareFunction(CompareFunction? comparison) => comparison switch
    {
        CompareFunction.Never => GL.GL_NEVER,
        CompareFunction.Less => GL.GL_LESS,
        CompareFunction.Equal => GL.GL_EQUAL,
        CompareFunction.LessEqual => GL.GL_LEQUAL,
        CompareFunction.Greater => GL.GL_GREATER,
        CompareFunction.NotEqual => GL.GL_NOTEQUAL,
        CompareFunction.GreaterEqual => GL.GL_GEQUAL,
        CompareFunction.Always => GL.GL_ALWAYS,
        null => GL.GL_ALWAYS,
        _ => throw new RenderException($"Unsupported comparison function {comparison}")
    };

    /// <summary>
    /// Disposes the sampler and releases its resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        uint handle = Handle;
        GL.glDeleteSamplers(1, ref handle);
        Disposed = true;
    }
}