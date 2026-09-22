using System;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLSampler : ISampler
{
    public uint Handle { get; }

    public TextureFilter MinFilter { get; }
    public TextureFilter MagFilter { get; }
    public MipmapFilter MipmapFilter { get; }

    public TextureWrap WrapU { get; }
    public TextureWrap WrapV { get; }
    public TextureWrap WrapW { get; }

    public AnisotropicFilter Anisotropy { get; }

    public CompareFunction? Comparison { get; }

    public bool Disposed { get; private set; }

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

    public void Dispose()
    {
        if (Disposed) return;
        uint handle = Handle;
        GL.glDeleteSamplers(1, ref handle);
        Disposed = true;
    }
}