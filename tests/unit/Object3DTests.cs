using System.Numerics;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class Object3DTests
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
    public void Shader_data_carries_inverse_transpose_normal_matrix_and_material()
    {
        Object3D obj = new()
        {
            Scale = new Vector3(2, 1, 1),
            Emissive = new Color(255, 0, 0),
            Shininess = 0f
        };

        ObjectShaderData data = obj.GetShaderData();

        Vector3 normal = Vector3.TransformNormal(new Vector3(1, 1, 0), data.NormalMatrix);
        Assert.Equal(new Vector3(0.5f, 1, 0), normal);
        Assert.Equal(new Vector4(1, 0, 0, 1), data.Emissive);
        Assert.Equal(1f, data.Shininess);
        Assert.False(data.Lit);
    }

    [Fact]
    public void World_bounds_transform_mesh_bounds()
    {
        using Mesh mesh = new(
        [
            new Vertex(new Vector3(-1, -2, -3), 0, 0),
            new Vertex(new Vector3(1, 2, 3), 1, 1),
            new Vertex(new Vector3(-1, 2, 3), 0, 1),
            new Vertex(new Vector3(1, -2, -3), 1, 0)
        ],
        [0, 1, 2]);
        using Object3D obj = new(mesh)
        {
            AutoDisposeMesh = false,
            Position = new Vector3(4, 5, 6),
            Scale = new Vector3(-2, 3, 1)
        };

        BoundingBox3D bounds = obj.GetWorldBounds();

        Assert.Equal(new Vector3(2, -1, 3), bounds.Min);
        Assert.Equal(new Vector3(6, 11, 9), bounds.Max);
    }

    [Fact]
    public void Model_world_bounds_union_transformed_parts()
    {
        Mesh firstMesh = MeshBuilder.CreatePlane(2, 2, 2);
        Mesh secondMesh = MeshBuilder.CreatePlane(2, 2, 2);
        using Model model = new(
            [firstMesh, secondMesh],
            [null, null],
            localTransforms:
            [
                Matrix4x4.CreateTranslation(-4, 0, 0),
                Matrix4x4.CreateTranslation(3, 0, 0)
            ])
        {
            Position = new Vector3(2, 0, 0)
        };

        BoundingBox3D bounds = model.GetWorldBounds();

        Assert.Equal(new Vector3(-2, 0, 0), bounds.Min);
        Assert.Equal(new Vector3(7, 2, 2), bounds.Max);
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