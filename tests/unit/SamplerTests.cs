using odl3d.Renderer;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class SamplerTests : TestBase
{
    [Fact]
    public void Defaults_are_valid_renderer_sampler_values()
    {
        Sampler sampler = new();

        Assert.Equal(TextureFilter.Nearest, sampler.MinFilter);
        Assert.Equal(TextureFilter.Nearest, sampler.MagFilter);
        Assert.Equal(MipmapFilter.None, sampler.MipmapFilter);
        Assert.Equal(TextureWrap.Repeat, sampler.WrapU);
        Assert.Equal(TextureWrap.Repeat, sampler.WrapV);
        Assert.Equal(TextureWrap.Repeat, sampler.WrapW);
        Assert.Equal(AnisotropicFilter.None, sampler.Anisotropy);
        Assert.Null(sampler.Comparison);
    }
}