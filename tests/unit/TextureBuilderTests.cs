using odl3d;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class TextureBuilderTests : TestBase
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

    [Fact]
    public void CreateGradient_preserves_corner_colors()
    {
        Color topLeft = new(10, 20, 30, 40);
        Color topRight = new(50, 60, 70, 80);
        Color bottomLeft = new(90, 100, 110, 120);
        Color bottomRight = new(130, 140, 150, 160);
        using Texture texture = TextureBuilder.CreateGradient(2, 2, topLeft, topRight, bottomLeft, bottomRight);

        Assert.Equal(new byte[]
        {
            10, 20, 30, 40, 50, 60, 70, 80,
            90, 100, 110, 120, 130, 140, 150, 160
        }, texture.Pixels);
    }

    [Fact]
    public void CreateCircle_fills_center_and_leaves_outside_transparent()
    {
        Color fill = new(25, 50, 75, 255);
        using Texture texture = TextureBuilder.CreateCircle(1, fill);

        Assert.Equal(fill, new Color(texture.Pixels[16], texture.Pixels[17], texture.Pixels[18], texture.Pixels[19]));
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, texture.Pixels[..4]);
    }

    [Fact]
    public void Texture_rejects_zero_dimensions_or_wrong_initial_pixel_count()
    {
        Assert.Throws<TextureException>(() => new Texture(0, 1));
        Assert.Throws<TextureException>(() => new Texture(1, 1, new byte[3]));
    }

    [Fact]
    public void Texture_pixels_are_rgba_and_top_to_bottom()
    {
        using Texture texture = new(2, 2);
        texture.SetPixel(0, 0, 1, 2, 3, 4);
        texture.SetPixel(1, 1, 5, 6, 7, 8);

        Assert.Equal(new byte[]
        {
            1, 2, 3, 4, 0, 0, 0, 0,
            0, 0, 0, 0, 5, 6, 7, 8
        }, texture.Pixels);
    }

    [Fact]
    public void HasPartialAlpha_reflects_pixel_alpha_changes()
    {
        using Texture texture = new(1, 1, new byte[] { 10, 20, 30, 128 });
        Assert.True(texture.HasPartialAlpha);

        texture.SetPixel(0, 0, 10, 20, 30, 255);

        Assert.False(texture.HasPartialAlpha);
    }

    [Fact]
    public void SetPixel_rejects_coordinates_outside_texture()
    {
        using Texture texture = new(1, 1);

        Assert.Throws<TextureException>(() => texture.SetPixel(-1, 0, 0, 0, 0));
        Assert.Throws<TextureException>(() => texture.SetPixel(1, 0, 0, 0, 0));
    }

    [Fact]
    public void Dispose_is_idempotent_and_raises_event_once()
    {
        Texture texture = new(1, 1);
        int disposeCount = 0;
        texture.OnDisposed += () => disposeCount++;

        texture.Dispose();
        texture.Dispose();

        Assert.True(texture.Disposed);
        Assert.Equal(1, disposeCount);
    }
}