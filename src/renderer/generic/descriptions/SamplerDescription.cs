using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the description of a texture sampler, including filtering, wrapping, anisotropy, and comparison settings.
/// </summary>
public sealed record SamplerDescription
{
    /// <summary>
    /// The minification filter used by the sampler.
    /// </summary>
    public TextureFilter MinFilter { get; init; } = TextureFilter.Nearest;

    /// <summary>
    /// The magnification filter used by the sampler.
    /// </summary>
    public TextureFilter MagFilter { get; init; } = TextureFilter.Nearest;

    /// <summary>
    /// The mipmap filter used by the sampler.
    /// </summary>
    public MipmapFilter MipmapFilter { get; init; } = MipmapFilter.None;

    /// <summary>
    /// The wrapping mode for the U (or S) texture coordinate.
    /// </summary>
    public TextureWrap WrapU { get; init; } = TextureWrap.Repeat;

    /// <summary>
    /// The wrapping mode for the V (or T) texture coordinate.
    /// </summary>
    public TextureWrap WrapV { get; init; } = TextureWrap.Repeat;

    /// <summary>
    /// The wrapping mode for the W (or R) texture coordinate.
    /// </summary>
    public TextureWrap WrapW { get; init; } = TextureWrap.Repeat;

    /// <summary>
    /// The anisotropic filtering mode used by the sampler.
    /// </summary>
    public AnisotropicFilter Anisotropy { get; init; } = AnisotropicFilter.None;

    /// <summary>
    /// The comparison function used by the sampler, if any.
    /// </summary>
    public CompareFunction? Comparison { get; init; }
}