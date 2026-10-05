using odl3d;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class MeshBuilderTests
{
    [Fact]
    public void CreateSphere_reports_expected_vertex_and_index_counts()
    {
        using Mesh mesh = MeshBuilder.CreateSphere(1f, 4);

        Assert.Equal(25, mesh.VertexCount);
        Assert.True(mesh.HasNormals);
    }

    [Fact]
    public void CreateQuad_builds_two_triangles_with_four_vertices()
    {
        using Mesh mesh = MeshBuilder.CreateQuad();

        Assert.Equal(4, mesh.VertexCount);
        Assert.False(mesh.HasNormals);
    }

    [Fact]
    public void CreateSphere_rejects_invalid_parameters()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MeshBuilder.CreateSphere(0f, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => MeshBuilder.CreateSphere(1f, 2));
    }

    [Fact]
    public void AddFace_reuses_vertices_and_emits_triangle_indices()
    {
        MeshBuilder builder = new();
        Vertex first = new(0, 0, 0);
        Vertex second = new(1, 0, 0);
        Vertex third = new(0, 1, 0);

        builder.AddFace(first, second, third);
        builder.AddFace(first, third, second);

        Assert.Equal(3, builder.Vertices.Count);
        Assert.Equal(new uint[] { 0, 1, 2, 0, 2, 1 }, builder.Indices);
        using Mesh mesh = builder.Build();
        Assert.Equal(3, mesh.VertexCount);
        Assert.False(mesh.HasNormals);
    }
}