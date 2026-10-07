using odl3d.Loaders;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class DaeLoaderTests
{
    [Fact]
    public void Load_preserves_normals_and_splits_vertices_with_different_normal_indices()
    {
        string path = Path.Combine(Path.GetTempPath(), $"odl3d-{Guid.NewGuid():N}.dae");
        File.WriteAllText(path, """
            <COLLADA>
              <library_geometries>
                <geometry id="geometry">
                  <mesh>
                    <source id="positions">
                      <float_array id="positions-array" count="9">0 0 0 1 0 0 0 1 0</float_array>
                      <technique_common><accessor source="#positions-array" count="3" stride="3" /></technique_common>
                    </source>
                    <source id="texcoords">
                      <float_array id="texcoords-array" count="2">0 0</float_array>
                      <technique_common><accessor source="#texcoords-array" count="1" stride="2" /></technique_common>
                    </source>
                    <source id="normals">
                      <float_array id="normals-array" count="6">0 0 1 0 1 0</float_array>
                      <technique_common><accessor source="#normals-array" count="2" stride="3" /></technique_common>
                    </source>
                    <vertices id="vertices">
                      <input semantic="POSITION" source="#positions" />
                    </vertices>
                    <triangles count="2">
                      <input semantic="VERTEX" source="#vertices" offset="0" />
                      <input semantic="NORMAL" source="#normals" offset="1" />
                      <input semantic="TEXCOORD" source="#texcoords" offset="2" />
                      <p>0 0 0 1 0 0 2 0 0 0 1 0 2 0 0 1 0 0</p>
                    </triangles>
                  </mesh>
                </geometry>
              </library_geometries>
            </COLLADA>
            """);

        try
        {
            (Mesh[] meshes, _, _, _) = DaeLoader.Load(path);
            using Mesh mesh = Assert.Single(meshes);

            Assert.True(mesh.HasNormals);
            Assert.Equal(4, mesh.VertexCount);
        }
        finally
        {
            File.Delete(path);
        }
    }
}