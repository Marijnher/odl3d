using System;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// Represents a texture sampler, which defines how textures are sampled and filtered during rendering.
/// </summary>
public class Sampler
{
    /// <summary>
    /// Gets the renderer associated with the current window.
    /// </summary>
    protected IRenderDevice Renderer => Window.Renderer;

    /// <summary>
    /// Gets the underlying render sampler used by the renderer.
    /// </summary>
    internal ISampler RenderSampler;

    private TextureFilter _minFilter;
    /// <summary>
    /// Gets or sets the minification filter for the texture sampler.
    /// </summary>
    public TextureFilter MinFilter
    {
        get => _minFilter;
        set
        {
            if (_minFilter.Equals(value)) return;
            _minFilter = value;
            InvalidateSampler();
        }
    }
    private TextureFilter _magFilter;
    /// <summary>
    /// Gets or sets the magnification filter for the texture sampler.
    /// </summary>
    public TextureFilter MagFilter
    {
        get => _magFilter;
        set
        {
            if (_magFilter.Equals(value)) return;
            _magFilter = value;
            InvalidateSampler();
        }
    }
    private MipmapFilter _mipmapFilter;
    /// <summary>
    /// Gets or sets the mipmap filter for the texture sampler.
    /// </summary>
    public MipmapFilter MipmapFilter
    {
        get => _mipmapFilter;
        set
        {
            if (_mipmapFilter.Equals(value)) return;
            _mipmapFilter = value;
            InvalidateSampler();
        }
    }

    private TextureWrap _wrapU;
    /// <summary>
    /// Gets or sets the wrap mode for the U (horizontal) texture coordinate.
    /// </summary>
    public TextureWrap WrapU
    {
        get => _wrapU;
        set
        {
            if (_wrapU.Equals(value)) return;
            _wrapU = value;
            InvalidateSampler();
        }
    }
    private TextureWrap _wrapV;
    /// <summary>
    /// Gets or sets the wrap mode for the V (vertical) texture coordinate.
    /// </summary>
    public TextureWrap WrapV
    {
        get => _wrapV;
        set
        {
            if (_wrapV.Equals(value)) return;
            _wrapV = value;
            InvalidateSampler();
        }
    }
    private TextureWrap _wrapW;
    /// <summary>
    /// Gets or sets the wrap mode for the W (depth) texture coordinate.
    /// </summary>
    public TextureWrap WrapW
    {
        get => _wrapW;
        set
        {
            if (_wrapW.Equals(value)) return;
            _wrapW = value;
            InvalidateSampler();
        }
    }

    private AnisotropicFilter _anistropy;
    /// <summary>
    /// Gets or sets the anisotropic filter for the texture sampler.
    /// </summary>
    public AnisotropicFilter Anisotropy
    {
        get => _anistropy;
        set
        {
            if (_anistropy.Equals(value)) return;
            _anistropy = value;
            InvalidateSampler();
        }
    }

    private CompareFunction? _comparison;
    /// <summary>
    /// Gets or sets the comparison function for the texture sampler.
    /// </summary>
    public CompareFunction? Comparison
    {
        get => _comparison;
        set
        {
            if (_comparison.Equals(value)) return;
            _comparison = value;
            InvalidateSampler();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sampler"/> class.
    /// </summary>
    public Sampler()
    {
        RenderSampler = CreateSampler();
    }

    /// <summary>
    /// Invalidates the current sampler and creates a new one.
    /// </summary>
    private void InvalidateSampler()
    {
        RenderSampler?.Dispose();
        RenderSampler = CreateSampler();
    }

    /// <summary>
    /// Creates a new texture sampler based on the current sampler settings.
    /// </summary>
    /// <returns>A new instance of <see cref="ISampler"/> based on the current sampler settings.</returns>
    private ISampler CreateSampler() => Renderer.CreateSampler(new SamplerDescription
    {
        MinFilter = MinFilter,
        MagFilter = MagFilter,
        MipmapFilter = MipmapFilter,
        WrapU = WrapU,
        WrapV = WrapV,
        WrapW = WrapW,
        Anisotropy = Anisotropy,
        Comparison = Comparison
    });
}