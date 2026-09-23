using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents a texture sampler that defines how textures are sampled, including filtering, wrapping, anisotropy, and comparison settings.
/// </summary>
public interface ISampler : IGPUResource
{
    /// <summary>
    /// Gets the minification filter used by the texture sampler.
    /// </summary>
    TextureFilter MinFilter { get; }

    /// <summary>
    /// Gets the magnification filter used by the texture sampler.
    /// </summary>
    TextureFilter MagFilter { get; }

    /// <summary>
    /// Gets the mipmap filter used by the texture sampler.
    /// </summary>
    MipmapFilter MipmapFilter { get; }

    /// <summary>
    /// Gets the wrapping mode for the U (horizontal) texture coordinate.
    /// </summary>
    TextureWrap WrapU { get; }

    /// <summary>
    /// Gets the wrapping mode for the V (vertical) texture coordinate.
    /// </summary>
    TextureWrap WrapV { get; }

    /// <summary>
    /// Gets the wrapping mode for the W (depth) texture coordinate.
    /// </summary>
    TextureWrap WrapW { get; }

    /// <summary>
    /// Gets the anisotropic filtering mode used by the texture sampler.
    /// </summary>
    AnisotropicFilter Anisotropy { get; }

    /// <summary>
    /// Gets the comparison function used by the texture sampler, if any.
    /// </summary>
    CompareFunction? Comparison { get; }
}