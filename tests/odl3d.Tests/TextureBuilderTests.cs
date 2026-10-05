using odl3d;
using Xunit;

namespace odl3d.Tests;

public class TextureBuilderTests
{
    [Fact]
    public void CreateSolid_preserves_dimensions_and_pixel_color()
    {
        Color expected = new(17, 33, 65, 129);
        using Texture texture = TextureBuilder.CreateSolid(2, 1, expected);

        Assert.Equal(2u, texture.Width);
        Assert.Equal(1u, texture.Height);
        Assert.Equal(new byte[] { 17, 33, 65, 129, 17, 33, 65, 129 }, texture.Pixels);
    }

    [Fact]
    public void CreateCheckerboard_alternates_colors_by_cell()
    {
        Color first = new(255, 0, 0);
        Color second = new(0, 0, 255);
        using Texture texture = TextureBuilder.CreateCheckerboard(2, 2, first, second, 1);

        Assert.Equal(new byte[]
        {
            255, 0, 0, 255, 0, 0, 255, 255,
            0, 0, 255, 255, 255, 0, 0, 255
        }, texture.Pixels);
    }
}