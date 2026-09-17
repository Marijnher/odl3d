using System;

namespace odl3d.Renderer;

public interface ISampler : IGPUResource
{
    TextureFilter MinFilter { get; }
    TextureFilter MagFilter { get; }
    MipmapFilter MipmapFilter { get; }

    TextureWrap WrapU { get; }
    TextureWrap WrapV { get; }
    TextureWrap WrapW { get; }

    AnisotropicFilter Anisotropy { get; }

    CompareFunction? Comparison { get; }
}