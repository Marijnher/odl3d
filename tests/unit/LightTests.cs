using System.Numerics;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class LightTests
{
    private static readonly Vector3 NoOffset = Vector3.Zero;

    [Fact]
    public void PointLight_packs_position_color_intensity_and_range()
    {
        PointLight light = new(new Vector3(1, 2, 3), new Color(255, 0, 0), intensity: 2f, range: 10f);

        LightShaderData data = light.ToShaderData(new Vector3(0, 10, 0));

        Assert.Equal(new Vector4(1, 12, 3, (float) LightKind.Point), data.PositionKind);
        Assert.Equal(new Vector4(2, 0, 0, 10), data.ColorRange);
    }

    [Fact]
    public void Negative_range_and_intensity_are_clamped()
    {
        PointLight light = new() { Range = -5f, Intensity = -1f };

        LightShaderData data = light.ToShaderData(NoOffset);

        Assert.Equal(new Vector4(0, 0, 0, 0), data.ColorRange);
    }

    [Fact]
    public void DirectionalLight_normalizes_direction_and_falls_back_when_zero()
    {
        DirectionalLight light = new(new Vector3(0, 0, -4));
        Assert.Equal(new Vector3(0, 0, -1), new Vector3(light.ToShaderData(NoOffset).DirectionA.X, light.ToShaderData(NoOffset).DirectionA.Y, light.ToShaderData(NoOffset).DirectionA.Z));

        light.Direction = Vector3.Zero;
        Assert.Equal(-1f, light.ToShaderData(NoOffset).DirectionA.Y);
        Assert.Equal(float.MaxValue, light.GetPriority(Vector3.Zero, NoOffset));
    }

    [Fact]
    public void SpotLight_orders_cone_angles_so_the_inner_cone_is_never_wider()
    {
        SpotLight light = new() { InnerAngle = 50f, OuterAngle = 30f };

        LightShaderData data = light.ToShaderData(NoOffset);

        Assert.True(data.TangentB.W > data.DirectionA.W, "cos(inner) must exceed cos(outer) for the shader's smoothstep.");
    }

    [Fact]
    public void AreaLight_packs_axes_half_sizes_and_flags()
    {
        AreaLight light = new(new Vector3(0, 5, 0), 4f, 2f) { Rotation = new Vector3(0, 90, 0), TwoSided = true, Shape = AreaLightShape.Disc };

        LightShaderData data = light.ToShaderData(NoOffset);

        Assert.Equal((float) LightKind.Disc + LightShaderData.TwoSidedFlag, data.PositionKind.W);
        Assert.Equal(2f, data.DirectionA.W);
        Assert.Equal(1f, data.TangentB.W);
        Assert.Equal(1f, new Vector3(data.DirectionA.X, data.DirectionA.Y, data.DirectionA.Z).Length(), 4);
        Assert.Equal(0f, Vector3.Dot(light.Normal, new Vector3(data.TangentB.X, data.TangentB.Y, data.TangentB.Z)), 4);
        Assert.Equal(1f, light.Normal.X, 4);
    }

    [Fact]
    public void Nearer_point_lights_have_higher_priority()
    {
        PointLight near = new(new Vector3(1, 0, 0));
        PointLight far = new(new Vector3(20, 0, 0));

        Assert.True(near.GetPriority(Vector3.Zero, NoOffset) > far.GetPriority(Vector3.Zero, NoOffset));
    }
}
