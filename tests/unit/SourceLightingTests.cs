using System.Numerics;
using odl3d.Loaders;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class SourceLightingTests
{
    private const string Dae = """
        <COLLADA>
          <library_lights>
            <light id="lamp"><technique_common><point><color>1 0.5 0</color></point></technique_common></light>
            <light id="sun"><technique_common><directional><color>1 1 1</color></directional></technique_common></light>
          </library_lights>
          <library_effects>
            <effect id="fx"><profile_COMMON><technique><phong>
              <emission><color>0.2 0 0 1</color></emission>
              <diffuse><color>0.5 0.5 0.5 1</color></diffuse>
              <specular><color>1 1 1 1</color></specular>
              <shininess><float>80</float></shininess>
            </phong></technique></profile_COMMON></effect>
          </library_effects>
          <library_materials>
            <material id="mat"><instance_effect url="#fx" /></material>
          </library_materials>
          <library_geometries>
            <geometry id="geo"><mesh>
              <source id="p"><float_array id="pa" count="9">0 0 0 1 0 0 0 1 0</float_array>
                <technique_common><accessor source="#pa" count="3" stride="3" /></technique_common></source>
              <source id="t"><float_array id="ta" count="2">0 0</float_array>
                <technique_common><accessor source="#ta" count="1" stride="2" /></technique_common></source>
              <vertices id="v"><input semantic="POSITION" source="#p" /></vertices>
              <triangles count="1" material="m"><input semantic="VERTEX" source="#v" offset="0" /><input semantic="TEXCOORD" source="#t" offset="1" /><p>0 0 1 0 2 0</p></triangles>
            </mesh></geometry>
          </library_geometries>
          <library_visual_scenes>
            <visual_scene id="s">
              <node><translate>0 50 0</translate><instance_light url="#lamp" /></node>
              <node><instance_light url="#sun" /></node>
              <node><instance_geometry url="#geo"><bind_material><technique_common>
                <instance_material symbol="m" target="#mat" /></technique_common></bind_material></instance_geometry></node>
            </visual_scene>
          </library_visual_scenes>
        </COLLADA>
        """;

    private static DaeLoader.DaeModelData Load()
    {
        string path = Path.Combine(Path.GetTempPath(), $"odl3d-{Guid.NewGuid():N}.dae");
        File.WriteAllText(path, Dae);
        try { return DaeLoader.LoadModelData(path); }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Dae_effect_lighting_properties_are_read()
    {
        SourceMaterial material = Assert.Single(Load().Materials);
        Assert.Equal(new Color(51, 0, 0), material.Emissive);
        Assert.Equal(Color.White, material.Specular);
        Assert.Equal(80f, material.Shininess);
        Assert.Equal(new Color(128, 128, 128), material.Diffuse);
    }

    [Fact]
    public void Dae_lights_are_read_with_their_node_transform()
    {
        SourceLight[] lights = Load().Lights;
        Assert.Equal(2, lights.Length);
        Assert.Equal(SourceLightKind.Point, lights[0].Kind);
        Assert.Equal(new Color(255, 128, 0), lights[0].Color);
        Assert.Equal(new Vector3(0, 1, 0), lights[0].Transform.Translation);
        Assert.Equal(SourceLightKind.Directional, lights[1].Kind);
    }

    [Fact]
    public void Source_lights_follow_the_model_matrix()
    {
        SourceLight source = Load().Lights[0];
        PointLight light = Assert.IsType<PointLight>(source.CreateLight(Matrix4x4.CreateTranslation(2, 0, 0), new Vector3(1, 0, 0)));
        Assert.Equal(new Vector3(1, 1, 0), light.Position);
    }

    [Fact]
    public void Mtl_emissive_color_is_read()
    {
        string path = Path.Combine(Path.GetTempPath(), $"odl3d-{Guid.NewGuid():N}.mtl");
        File.WriteAllText(path, "newmtl glow\nKe 1 0 0\nKs 0.5 0.5 0.5\nNs 10\n");
        try
        {
            Material material = MtlLoader.Load(path)["glow"];
            Assert.Equal(new[] { 1f, 0f, 0f }, material.Ke);
        }
        finally { File.Delete(path); }
    }
}
