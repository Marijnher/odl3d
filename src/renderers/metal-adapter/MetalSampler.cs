using System;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a Metal sampler state and provides access to its properties.
/// </summary>
internal class MetalSampler : ISampler
{
    /// <summary>
    /// Gets the underlying Metal sampler state.
    /// </summary>
    public Metal.SamplerState Sampler;

    /// <summary>
    /// Gets the minification filter of the sampler.
    /// </summary>
    public TextureFilter MinFilter { get; }

    /// <summary>
    /// Gets the magnification filter of the sampler.
    /// </summary>
    public TextureFilter MagFilter { get; }

    /// <summary>
    /// Gets the mipmap filter of the sampler.
    /// </summary>
    public MipmapFilter MipmapFilter { get; }

    /// <summary>
    /// Gets the wrap mode for the U texture coordinate.
    /// </summary>
    public TextureWrap WrapU { get; }

    /// <summary>
    /// Gets the wrap mode for the V texture coordinate.
    /// </summary>
    public TextureWrap WrapV { get; }

    /// <summary>
    /// Gets the wrap mode for the W texture coordinate.
    /// </summary>
    public TextureWrap WrapW { get; }

    /// <summary>
    /// Gets the anisotropic filter of the sampler.
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
    /// Initializes a new instance of the <see cref="MetalSampler"/> class.
    /// </summary>
    /// <param name="device">The Metal device used to create the sampler.</param>
    /// <param name="description">The description of the sampler state.</param>
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

    /// <summary>
    /// Disposes the sampler and releases its resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Sampler.Dispose();
        Disposed = true;
    }
}