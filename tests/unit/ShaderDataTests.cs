using System.Runtime.InteropServices;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class ShaderDataTests
{
    [Fact]
    public void Shader_data_sizes_and_field_offsets_match_gpu_layout()
    {
        Assert.Equal(256, Marshal.SizeOf<ObjectShaderData>());
        Assert.Equal(128, Marshal.SizeOf<SceneShaderData>());
        Assert.Equal(0, Marshal.OffsetOf<ObjectShaderData>(nameof(ObjectShaderData.Model)).ToInt32());
        Assert.Equal(64, Marshal.OffsetOf<ObjectShaderData>(nameof(ObjectShaderData.TexColor)).ToInt32());
        Assert.Equal(80, Marshal.OffsetOf<ObjectShaderData>(nameof(ObjectShaderData.ObjColor)).ToInt32());
        Assert.Equal(96, Marshal.OffsetOf<ObjectShaderData>("_useTexture").ToInt32());
        Assert.Equal(100, Marshal.OffsetOf<ObjectShaderData>("_hasNormals").ToInt32());
        Assert.Equal(104, Marshal.OffsetOf<ObjectShaderData>("_lit").ToInt32());
        Assert.Equal(108, Marshal.OffsetOf<ObjectShaderData>(nameof(ObjectShaderData.Shininess)).ToInt32());
        Assert.Equal(112, Marshal.OffsetOf<ObjectShaderData>(nameof(ObjectShaderData.Emissive)).ToInt32());
        Assert.Equal(128, Marshal.OffsetOf<ObjectShaderData>(nameof(ObjectShaderData.Specular)).ToInt32());
        Assert.Equal(144, Marshal.OffsetOf<ObjectShaderData>(nameof(ObjectShaderData.NormalMatrix)).ToInt32());
        Assert.Equal(0, Marshal.OffsetOf<SceneShaderData>(nameof(SceneShaderData.Projection)).ToInt32());
        Assert.Equal(64, Marshal.OffsetOf<SceneShaderData>(nameof(SceneShaderData.View)).ToInt32());
    }

    [Fact]
    public void Lighting_data_sizes_and_field_offsets_match_gpu_layout()
    {
        Assert.Equal(64, Marshal.SizeOf<LightShaderData>());
        Assert.Equal(16, Marshal.OffsetOf<LightShaderData>(nameof(LightShaderData.ColorRange)).ToInt32());
        Assert.Equal(32, Marshal.OffsetOf<LightShaderData>(nameof(LightShaderData.DirectionA)).ToInt32());
        Assert.Equal(48, Marshal.OffsetOf<LightShaderData>(nameof(LightShaderData.TangentB)).ToInt32());
        Assert.Equal(48, Marshal.SizeOf<LightingShaderData>());
        Assert.Equal(16, Marshal.OffsetOf<LightingShaderData>(nameof(LightingShaderData.Ambient)).ToInt32());
        Assert.Equal(32, Marshal.OffsetOf<LightingShaderData>(nameof(LightingShaderData.LightCount)).ToInt32());
    }

    [Fact]
    public void Default_shaders_declare_matching_object_and_lighting_blocks()
    {
        string glslVertex = DefaultShaders.Vertex(RenderTarget.OpenGL);
        string glslFragment = DefaultShaders.Fragment(RenderTarget.OpenGL);
        string metalVertex = DefaultShaders.Vertex(RenderTarget.Metal);
        string metalFragment = DefaultShaders.Fragment(RenderTarget.Metal);

        foreach (string member in new[] { "lit", "shininess", "emissive", "specular", "normalMatrix" })
        {
            Assert.Contains(member + ";", glslVertex);
            Assert.Contains(member + ";", glslFragment);
            Assert.Contains(member + ";", metalVertex);
        }
        Assert.Contains("binding = 2) uniform LightingData", glslFragment);
        Assert.Contains("binding = 3) uniform LightList", glslFragment);
        Assert.Contains("__MAX_LIGHTS__", glslFragment);
        Assert.Contains("[[buffer(2)]]", metalFragment);
        Assert.Contains("[[buffer(3)]]", metalFragment);
        Assert.Equal(SceneLighting.HeaderSlot, 2);
        Assert.Equal(SceneLighting.ListSlot, 3);
    }

    [Fact]
    public void Default_shaders_use_matching_depth_ranges()
    {
        string glsl = DefaultShaders.Vertex(RenderTarget.OpenGL);
        string metal = DefaultShaders.Vertex(RenderTarget.Metal);

        Assert.Contains("gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;", glsl);
        Assert.DoesNotContain("out.position.z", metal);
    }

    [Fact]
    public void Default_shaders_reject_unknown_render_targets()
    {
        Assert.Throws<RenderException>(() => DefaultShaders.Vertex((RenderTarget)999));
        Assert.Throws<RenderException>(() => DefaultShaders.Fragment((RenderTarget)999));
    }
}