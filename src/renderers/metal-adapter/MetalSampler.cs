using System;

namespace odl3d.Renderer.MetalAdapter;

public class MetalSampler : ISampler
{
    public Metal.SamplerState Sampler;

    public TextureFilter MinFilter { get; }
    public TextureFilter MagFilter { get; }
    public MipmapFilter MipmapFilter { get; }

    public TextureWrap WrapU { get; }
    public TextureWrap WrapV { get; }
    public TextureWrap WrapW { get; }

    public AnisotropicFilter Anisotropy { get; }

    public CompareFunction? Comparison { get; }

    public bool Disposed { get; private set; }

    public MetalSampler(Metal.Device device, SamplerDescription description)
    {
        MinFilter = description.MinFilter;
        MagFilter = description.MagFilter;
        MipmapFilter = description.MipmapFilter;
        WrapU = description.WrapU;
        WrapV = description.WrapV;
        WrapW = description.WrapW;
        Anisotropy = description.Anisotropy;
        Comparison = description.Comparison;
        Sampler = device.CreateSampler(
            description.MinFilter,
            description.MagFilter,
            description.MipmapFilter,
            description.WrapU,
            description.WrapV,
            description.WrapW,
            description.Anisotropy
        );
    }

    public void Dispose()
    {
        if (Disposed) return;
        Sampler.Dispose();
        Disposed = true;
    }
}