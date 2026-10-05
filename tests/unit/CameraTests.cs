using System.Numerics;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class CameraTests
{
    [Theory]
    [InlineData(-180f, -89f)]
    [InlineData(-90f, 0f)]
    [InlineData(0f, 45f)]
    [InlineData(90f, 89f)]
    [InlineData(180f, -30f)]
    public void Camera_basis_is_orthonormal(float yaw, float pitch)
    {
        Camera camera = new(null!, 16f / 9f) { Yaw = yaw, Pitch = pitch };

        Assert.InRange(MathF.Abs(camera.Front.Length() - 1f), 0f, 1e-6f);
        Assert.InRange(Vector3.Dot(camera.Front, camera.Up), -1e-6f, 1e-6f);
        Assert.InRange(Vector3.Dot(camera.Front, camera.Right), -1e-6f, 1e-6f);
        Assert.InRange(Vector3.Dot(camera.Up, camera.Right), -1e-6f, 1e-6f);
        Assert.Equal(-camera.Right, camera.Left);
    }

    [Fact]
    public void Rotate_clamps_pitch_without_flipping()
    {
        Camera camera = new(null!, 1f);

        camera.Rotate(0, 1000);
        Assert.Equal(89f, camera.Pitch);

        camera.Rotate(0, -2000);
        Assert.Equal(-89f, camera.Pitch);
    }

    [Fact]
    public void View_matrix_maps_camera_position_to_origin_and_front_to_negative_z()
    {
        Camera camera = new(null!, 1f)
        {
            Position = new Vector3(3, 4, 5),
            Yaw = 20,
            Pitch = 15
        };
        Matrix4x4 view = camera.GetViewMatrix();

        Assert.InRange(Vector3.Distance(Vector3.Transform(camera.Position, view), Vector3.Zero), 0f, 1e-5f);
        Vector3 frontPoint = Vector3.Transform(camera.Position + camera.Front, view);
        Assert.InRange(frontPoint.X, -1e-5f, 1e-5f);
        Assert.InRange(frontPoint.Y, -1e-5f, 1e-5f);
        Assert.InRange(frontPoint.Z, -1.00001f, -0.99999f);
    }

    [Fact]
    public void Projection_maps_near_and_far_planes_to_zero_and_one_depth()
    {
        Camera camera = new(null!, 16f / 9f);
        Matrix4x4 projection = camera.GetProjectionMatrix();
        Vector4 nearClip = Vector4.Transform(new Vector4(0, 0, -camera.NearPlane, 1), projection);
        Vector4 farClip = Vector4.Transform(new Vector4(0, 0, -camera.FarPlane, 1), projection);

        Assert.InRange(nearClip.Z / nearClip.W, -1e-5f, 1e-5f);
        Assert.InRange(farClip.Z / farClip.W, 0.99999f, 1.00001f);
    }
}