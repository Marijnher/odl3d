using System;
using odl3d;
using odl3d.Renderers;

namespace odl3ddemo;

public static class Program
{
    private const string VertexSource = @"#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;
out vec2 vTexCoord;
out vec3 vNormal;
uniform mat4 uMVP;
uniform mat4 uModel;
void main()
{
    gl_Position = uMVP * vec4(aPos, 1.0);
    vTexCoord = aTexCoord;
    vNormal = mat3(uModel) * aNormal;
}";

    private const string FragmentSource = @"#version 330 core
in vec2 vTexCoord;
in vec3 vNormal;
out vec4 FragColor;
uniform sampler2D uTexture;
uniform bool uUseTexture;
uniform bool uLit;
uniform vec4 uColor;
uniform vec4 texColor;
void main()
{
    vec4 color = uUseTexture ? texture(uTexture, vTexCoord) * texColor : uColor;

    // Discard fully transparent fragments
    if (color.a == 0)
        discard;

    // Meshes with normals (e.g. extruded 3D text) get simple two-sided directional lighting.
    if (uLit)
    {
        vec3 normal = normalize(vNormal);
        vec3 lightDirection = normalize(vec3(-0.35, 0.6, 1.0));
        float diffuse = max(abs(dot(normal, lightDirection)), 0.0);
        color.rgb *= 0.35 + 0.65 * diffuse;
    }

    FragColor = color;
}";

    private const string MetalVertexSource = @"#include <metal_stdlib>
using namespace metal;

struct Uniforms {
    float4x4 mvp;
    float4x4 model;
    float4 color;
    float4 textureColor;
    int useTexture;
    int lit;
    int floatsPerVertex;
};

struct VertexOut {
    float4 position [[position]];
    float2 texCoord;
    float3 normal;
};

vertex VertexOut vertex_main(uint vertexId [[vertex_id]],
                             device const float* vertices [[buffer(0)]],
                             constant Uniforms& uniforms [[buffer(1)]]) {
    VertexOut out;
    uint offset = vertexId * uint(uniforms.floatsPerVertex);
    float3 position = float3(vertices[offset], vertices[offset + 1], vertices[offset + 2]);
    out.position = uniforms.mvp * float4(position, 1.0);
    out.texCoord = float2(vertices[offset + 3], vertices[offset + 4]);
    float3 normal = uniforms.floatsPerVertex == 8
        ? float3(vertices[offset + 5], vertices[offset + 6], vertices[offset + 7])
        : float3(0.0, 0.0, 1.0);
    out.normal = (uniforms.model * float4(normal, 0.0)).xyz;
    return out;
}";

    private const string MetalFragmentSource = @"#include <metal_stdlib>
using namespace metal;

struct Uniforms {
    float4x4 mvp;
    float4x4 model;
    float4 color;
    float4 textureColor;
    int useTexture;
    int lit;
    int floatsPerVertex;
};

struct VertexOut {
    float4 position [[position]];
    float2 texCoord;
    float3 normal;
};

fragment float4 fragment_main(VertexOut in [[stage_in]],
                              constant Uniforms& uniforms [[buffer(1)]],
                              texture2d<float> texture [[texture(0)]],
                              sampler textureSampler [[sampler(0)]]) {
    float4 color = uniforms.color;
    if (uniforms.useTexture != 0)
        color = texture.sample(textureSampler, in.texCoord) * uniforms.textureColor;
    if (color.a == 0.0) discard_fragment();
    if (uniforms.lit != 0) {
        float3 normal = normalize(in.normal);
        float3 lightDirection = normalize(float3(-0.35, 0.6, 1.0));
        float diffuse = max(abs(dot(normal, lightDirection)), 0.0);
        color.rgb *= 0.35 + 0.65 * diffuse;
    }
    return color;
}";

    public static void Main(string[] args)
    {
        MetalNew.Load();
        MetalNew.Device device = MetalNew.GetDefaultDevice();
        Console.WriteLine(device.Name);
        Console.WriteLine(device.Description);
        Console.WriteLine(device.RegistryID);

        MetalNew.CommandQueue queue = device.NewCommandQueue();
        Console.WriteLine($"Queue: {queue.Handle}");
        Console.WriteLine($"Label: {queue.Label}");
        queue.Label = "Hello";
        Console.WriteLine($"Label: {queue.Label}");

        return;

        int width = 800;
        int height = 600;

        GLFW.Load();
        RenderFactory.Create("opengl");
        
        Window window = new Window(width, height, "odl3d");
        window.BackgroundColor = new Color(0, 0, 0);
        window.Camera = new MoveableCamera(window);
        window.SetCursorCapture(true);
        window.RegisterKeyPress(Key.Escape, window.Close);
        window.RegisterKeyPress(Key.M, () => window.SetWireFrame(!window.Wireframe));

        bool metal = RenderFactory.Renderer is odl3d.Renderers.Metal;
        ShaderProgram shader = new ShaderProgram(
            metal ? MetalVertexSource : VertexSource,
            metal ? MetalFragmentSource : FragmentSource);

        DemoScene demoScene1 = new DemoScene(window);
        UIScene uiScene = new UIScene(window);
        ModelScene daeScene = new ModelScene(window);

        while (!window.ShouldClose)
        {
            window.Update(0);
            window.Render(shader);
            window.SwapBuffers();
        }

        window.Dispose();
        shader.Dispose();
    }
}
