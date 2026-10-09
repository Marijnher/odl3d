using System.Numerics;
using System.Runtime.InteropServices;
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
        Assert.True(mesh.HasNormals);
        Assert.True(mesh.HasGeneratedNormals);
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
        // The two faces wind oppositely, so their generated flat normals differ and the vertices are separated.
        Assert.Equal(6, mesh.VertexCount);
        Assert.True(mesh.HasGeneratedNormals);
    }

    [Fact]
    public void AddFace_deduplicates_only_vertices_with_matching_position_texcoord_and_normal()
    {
        MeshBuilder builder = new();
        Vertex first = new(new Vector3(1, 2, 3), new Vector3(0, 0, 1), 0.25f, 0.75f);
        Vertex same = new(new Vector3(1, 2, 3), new Vector3(0, 0, 1), 0.25f, 0.75f);
        Vertex differentNormal = new(new Vector3(1, 2, 3), new Vector3(0, 1, 0), 0.25f, 0.75f);
        Vertex second = new(0, 0, 0);
        Vertex third = new(0, 1, 0);

        builder.AddFace(first, second, third);
        builder.AddFace(same, second, third);
        builder.AddFace(differentNormal, second, third);

        Assert.Equal(4, builder.Vertices.Count);
        Assert.Equal(new uint[] { 0, 1, 2, 0, 1, 2, 3, 1, 2 }, builder.Indices);
    }

    [Fact]
    public void Vertex_has_the_expected_gpu_layout_and_zero_default_normal()
    {
        Assert.Equal(32, Marshal.SizeOf<Vertex>());
        Assert.Equal(0, Marshal.OffsetOf<Vertex>(nameof(Vertex.Position)).ToInt32());
        Assert.Equal(12, Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoord)).ToInt32());
        Assert.Equal(20, Marshal.OffsetOf<Vertex>(nameof(Vertex.Normal)).ToInt32());
        Assert.Equal(Vector3.Zero, new Vertex(Vector3.One, 0.5f, 0.5f).Normal);
    }
}