using System.Globalization;
using odl3d.Loaders;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class ObjLoaderTests
{
    [Fact]
    public void Load_triangulates_negative_indices_independent_of_current_culture()
    {
        string path = Path.Combine(Path.GetTempPath(), $"odl3d-{Guid.NewGuid():N}.obj");
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        File.WriteAllText(path, """
            o quad
            v 0.25 0 0
            v 1 0 0
            v 1 1 0
            v 0 1 0
            f -4 -3 -2 -1
            """);

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            ObjFile obj = ObjLoader.Load(path);
            using Mesh mesh = obj.Meshes["quad"].Mesh;

            Assert.Equal(4, mesh.VertexCount);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_preserves_normals_and_splits_vertices_with_different_normals()
    {
        string path = Path.Combine(Path.GetTempPath(), $"odl3d-{Guid.NewGuid():N}.obj");
        File.WriteAllText(path, """
            v 0 0 0
            v 1 0 0
            v 0 1 0
            vt 0 0
            vn 0 0 1
            vn 0 1 0
            f 1/1/1 2/1/1 3/1/1
            f 1/1/2 3/1/1 2/1/1
            """);

        try
        {
            ObjFile obj = ObjLoader.Load(path);
            using Mesh mesh = obj.Meshes["default"].Mesh;

            Assert.True(mesh.HasNormals);
            Assert.Equal(4, mesh.VertexCount);
        }
        finally
        {
            File.Delete(path);
        }
    }
}