using System.Runtime.InteropServices;
using Xunit;

namespace odl3d.Tests;

[Trait("Tier", "T0")]
public class ShaderDataTests : TestBase
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
        Assert.Equal(0, Marshal.OffsetOf<SceneShaderData>(nameof(SceneShaderData.Projection)).ToInt32());
        Assert.Equal(64, Marshal.OffsetOf<SceneShaderData>(nameof(SceneShaderData.View)).ToInt32());
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