using System;

namespace odl3d.Renderer;

public sealed record SamplerDescription
{
    public TextureFilter MinFilter { get; init; } = TextureFilter.Nearest;
    public TextureFilter MagFilter { get; init; } = TextureFilter.Nearest;
    public MipmapFilter MipmapFilter { get; init; } = MipmapFilter.None;

    public TextureWrap AddressU { get; init; } = TextureWrap.Repeat;
    public TextureWrap AddressV { get; init; } = TextureWrap.Repeat;
    public TextureWrap AddressW { get; init; } = TextureWrap.Repeat;

    public AnisotropicFilter Anisotropy { get; init; } = AnisotropicFilter.None;

    public CompareFunction? Comparison { get; init; }
}