using System.Numerics;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class FlatNormalsTests
{
    [Fact]
    public void Counter_clockwise_triangle_gets_its_outward_face_normal()
    {
        Vertex[] vertices = [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)];

        using Mesh mesh = new(vertices, [0, 1, 2]);

        Assert.True(mesh.HasGeneratedNormals);
        (Vertex[] result, _, _) = FlatNormals.Apply(vertices, [0, 1, 2], false);
        Assert.All(result, vertex => Assert.Equal(Vector3.UnitZ, vertex.Normal));
    }

    [Fact]
    public void Coplanar_triangles_keep_their_shared_vertices()
    {
        Vertex[] vertices = [new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)];

        (Vertex[] result, uint[] indices, bool generated) = FlatNormals.Apply(vertices, [0, 1, 2, 0, 2, 3], false);

        Assert.True(generated);
        Assert.Equal(4, result.Length);
        Assert.Equal(6, indices.Length);
    }

    [Fact]
    public void Vertices_shared_by_faces_with_different_normals_are_split()
    {
        Vertex[] vertices = [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)];

        (Vertex[] result, _, _) = FlatNormals.Apply(vertices, [0, 1, 2, 0, 3, 1], false);

        Assert.Equal(6, result.Length);
    }

    [Fact]
    public void Existing_normals_are_left_untouched()
    {
        Vertex[] vertices =
        [
            new(new Vector3(0, 0, 0), new Vector3(0, 1, 0)),
            new(new Vector3(1, 0, 0), new Vector3(0, 1, 0)),
            new(new Vector3(0, 1, 0), new Vector3(0, 1, 0))
        ];

        (Vertex[] result, uint[] indices, bool generated) = FlatNormals.Apply(vertices, [0, 1, 2], true);

        Assert.False(generated);
        Assert.Same(vertices, result);
        Assert.Equal(new uint[] { 0, 1, 2 }, indices);
    }

    [Fact]
    public void Only_triangles_with_a_missing_normal_are_rewritten()
    {
        Vector3 up = Vector3.UnitY;
        Vertex[] vertices =
        [
            new(new Vector3(0, 0, 0), up), new(new Vector3(1, 0, 0), up), new(new Vector3(0, 0, -1), up),
            new(0, 0, 5), new(1, 0, 5), new(0, 1, 5)
        ];

        (Vertex[] result, uint[] indices, bool generated) = FlatNormals.Apply(vertices, [0, 1, 2, 3, 4, 5], true);

        Assert.True(generated);
        Assert.Equal(up, result[indices[0]].Normal);
        Assert.Equal(Vector3.UnitZ, result[indices[3]].Normal);
    }

    [Fact]
    public void Degenerate_triangles_get_a_finite_unit_normal()
    {
        Vertex[] vertices = [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0)];

        (Vertex[] result, _, _) = FlatNormals.Apply(vertices, [0, 1, 2], false);

        Assert.All(result, vertex => Assert.Equal(1f, vertex.Normal.Length(), 4));
    }
}
