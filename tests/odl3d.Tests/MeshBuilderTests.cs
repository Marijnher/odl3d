using odl3d;
using Xunit;

namespace odl3d.Tests;

public class MeshBuilderTests
{
    [Fact]
    public void CreateSphere_reports_vertices_and_normals()
    {
        using Mesh mesh = MeshBuilder.CreateSphere(1f, 4);

        Assert.Equal(25, mesh.VertexCount);
        Assert.True(mesh.HasNormals);
    }
}