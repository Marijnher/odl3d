using System;
using odl3d.Renderer;

namespace odl3d;

public class Sampler
{
    protected IRenderDevice Renderer => Window.Renderer;

    public ISampler RenderSampler;

    private TextureFilter _minFilter;
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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Sampler()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        InvalidateSampler();
    }

    private void InvalidateSampler()
    {
        RenderSampler?.Dispose();
        RenderSampler = Renderer.CreateSampler(new SamplerDescription
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
}