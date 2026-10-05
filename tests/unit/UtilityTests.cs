using System.Numerics;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class UtilityTests : TestBase
{
    [Fact]
    public void Color_constants_and_vector_conversion_preserve_rgba()
    {
        Assert.Equal(new Color(255, 0, 0, 255), Color.Red);
        Assert.Equal(new Color(0, 255, 0, 255), Color.Green);
        Assert.Equal(new Color(0, 0, 255, 255), Color.Blue);
        Assert.Equal(new Color(0, 0, 0, 0), Color.Alpha);

        Color color = new(128, 64, 255, 32);
        Vector4 vector = color.ToVector4();

        Assert.Equal(128 / 255f, vector.X);
        Assert.Equal(64 / 255f, vector.Y);
        Assert.Equal(1f, vector.Z);
        Assert.Equal(32 / 255f, vector.W);
    }

    [Fact]
    public void Rect_contains_edges_and_zero_size_point()
    {
        Rect rect = new(2, 3, 4, 5);

        Assert.True(rect.Contains(2, 3));
        Assert.True(rect.Contains(6, 8));
        Assert.False(rect.Contains(6.01f, 8));
        Assert.True(new Rect(4, 7, 0, 0).Contains(4, 7));
    }

    [Fact]
    public void Rect_intersection_is_symmetric_and_includes_touching_edges()
    {
        Rect left = new(0, 0, 2, 2);
        Rect right = new(2, 0, 2, 2);

        Assert.True(left.Intersects(right));
        Assert.True(right.Intersects(left));
        Assert.False(left.Intersects(new Rect(2.01f, 0, 2, 2)));
        Assert.False(new Rect(2, 0, -1, 2).Contains(1.5f, 1));
        Assert.False(new Rect(2, 0, -1, 2).Intersects(left));
    }

    [Fact]
    public void Matrix_to_array_is_row_major()
    {
        Matrix4x4 matrix = new(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16);

        Assert.Equal(new float[]
        {
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16
        }, matrix.ToArray());
    }

    [Fact]
    public void Timer_uses_clock_for_remaining_time_and_fires_once_per_reset()
    {
        double now = 10;
        int finishedCount = 0;
        Timer timer = new(2, () => now) { OnFinished = () => finishedCount++ };

        Assert.Equal(2, timer.RemainingTime);
        Assert.False(timer.IsFinished);
        now = 12;
        Assert.Equal(0, timer.RemainingTime);
        Assert.True(timer.IsFinished);

        timer.Update();
        timer.Update();
        Assert.Equal(1, finishedCount);

        now = 20;
        timer.Reset(3);
        Assert.Equal(3, timer.RemainingTime);
        Assert.False(timer.CalledOnFinished);
    }
}