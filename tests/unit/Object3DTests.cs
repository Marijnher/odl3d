using System.Numerics;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class Object3DTests : TestBase
{
    [Fact]
    public void Model_matrix_applies_scale_before_translation()
    {
        Object3D obj = new()
        {
            Position = new Vector3(1, 2, 3),
            Scale = new Vector3(2, 3, 4)
        };

        Vector3 transformed = Vector3.Transform(Vector3.UnitX, obj.GetModelMatrix());

        Assert.Equal(new Vector3(3, 2, 3), transformed);
    }

    [Fact]
    public void Dispose_respects_resource_ownership_flags()
    {
        using Mesh sharedMesh = MeshBuilder.CreateQuad();
        using Texture sharedTexture = TextureBuilder.CreateSolid(1, 1, Color.White);
        Object3D obj = new(sharedMesh, sharedTexture)
        {
            AutoDisposeMesh = false,
            AutoDisposeTexture = false
        };

        obj.Dispose();

        Assert.False(sharedMesh.Disposed);
        Assert.False(sharedTexture.Disposed);
    }

    [Fact]
    public void Dispose_releases_owned_resources()
    {
        Mesh mesh = MeshBuilder.CreateQuad();
        Texture texture = TextureBuilder.CreateSolid(1, 1, Color.White);
        Object3D obj = new(mesh, texture);

        obj.Dispose();

        Assert.True(mesh.Disposed);
        Assert.True(texture.Disposed);
    }
}