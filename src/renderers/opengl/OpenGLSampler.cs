using System;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLSampler : ISampler
{
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
    }

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
    }
}