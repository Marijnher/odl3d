using System;
using System.Collections.Generic;
using System.Numerics;
using odl3d;
using odl3d.Renderer;
using odl3d.Renderer.MetalAdapter;

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

    private const string SimpleMetalVertexSource = @"#include <metal_stdlib>
using namespace metal;

struct VertexIn
{
    float3 position [[attribute(0)]];
    float2 texCoord [[attribute(1)]];
};

struct VertexOut
{
    float4 position [[position]];
    float2 texCoord;
};

vertex VertexOut vertex_main(
                    uint vertexID [[vertex_id]],
                    VertexIn in [[stage_in]],
                    constant float4x4& camera [[buffer(1)]]
                )
{
    VertexOut out;
    out.position = camera * float4(in.position, 1.0);
    out.texCoord = in.texCoord;
    return out;
}";

    private const string SimpleMetalFragmentSource = @"#include <metal_stdlib>
using namespace metal;

fragment float4 fragment_main(
                    VertexOut in [[stage_in]],
                    texture2d<float> texture [[texture(0)]],
                    sampler sampler [[sampler(0)]]
                )
{
    float4 color = texture.sample(sampler, in.texCoord);
    return color;
}";

    private const string SimplerMetalVertexSource = @"#include <metal_stdlib>
using namespace metal;

struct VertexIn
{
    packed_float3 position;
    packed_float2 texCoord;
};

struct VertexOut
{
    float4 position [[position]];
    float2 texCoord;
};

vertex VertexOut vertex_main(
                    uint vertexID [[vertex_id]],
                    device const VertexIn* vertices [[buffer(0)]],
                    constant float4x4& camera [[buffer(1)]]
                )
{
    VertexOut out;
    out.position = camera * float4(vertices[vertexID].position, 1.0);
    out.texCoord = vertices[vertexID].texCoord;
    return out;
}";

    private const string SimplerMetalFragmentSource = @"#include <metal_stdlib>
using namespace metal;

fragment float4 fragment_main(
                    VertexOut in [[stage_in]]
                )
{
    float4 color = float4(in.texCoord, 1.0, 1.0);
    return color;
}";

    public static void Main(string[] args)
    {
        using IRenderDevice device = new MetalRenderDevice();
        Console.WriteLine(device.Name);

        GLFW.Load();
        GLFW.glfwWindowHint(GLFW.GLFW_RESIZABLE, GLFW.GLFW_TRUE);
        int winWidth = 400;
        int winHeight = 400;
        nint winHandle = GLFW.glfwCreateWindow(winWidth, winHeight, "Metal", IntPtr.Zero, IntPtr.Zero);
        if (winHandle == IntPtr.Zero)
        {
            GLFW.glfwTerminate();
            throw new Exception("Failed to create a GLFW window.");
        }

        using IRenderSurface surface = device.CreateSurface(winHandle);

        var vertexLayout = new VertexLayoutDescription
        {
            Buffers = [
                new VertexBufferLayoutDescription // Buffer 0
                {
                    BufferIndex = 0,
                    Stride = 5 * sizeof(float),
                    StepFunction = StepMode.PerVertex
                }
            ],
            Attributes = [
                new VertexAttributeDescription // Atribute 0 (position, float3)
                {
                    AttributeIndex = 0,
                    Format = VertexFormat.Float3,
                    Offset = 0,
                    BufferSlot = 0
                },
                new VertexAttributeDescription // Attribute 1 (texCoord, float2)
                {
                    AttributeIndex = 1,
                    Format = VertexFormat.Float2,
                    Offset = 3 * sizeof(float),
                    BufferSlot = 0
                }
            ]
        };
        using IShaderModule vertexShader = device.CreateShaderModule(new ShaderModuleDescription
        {
            Stage = ShaderStage.Vertex,
            Source = SimpleMetalVertexSource,
            EntryPoint = "vertex_main"
        });
        using IShaderModule fragmentShader = device.CreateShaderModule(new ShaderModuleDescription
        {
            Stage = ShaderStage.Fragment,
            Source = SimpleMetalFragmentSource,
            EntryPoint = "fragment_main"
        });
        using IRenderPipeline renderPipeline = device.CreateRenderPipeline(new RenderPipelineDescription()
        {
            VertexShader = vertexShader,
            FragmentShader = fragmentShader,
            VertexLayout = vertexLayout,
            ColorFormat = TextureFormat.RGBA32Float
        });

        using IShaderModule simpleVertex = device.CreateShaderModule(new ShaderModuleDescription
        {
            Stage = ShaderStage.Vertex,
            Source = SimplerMetalVertexSource,
            EntryPoint = "vertex_main"
        });
        using IShaderModule simpleFragment = device.CreateShaderModule(new ShaderModuleDescription
        {
            Stage = ShaderStage.Fragment,
            Source = SimplerMetalFragmentSource,
            EntryPoint = "fragment_main"
        });
        using IRenderPipeline simpleRenderPipleline = device.CreateRenderPipeline(new RenderPipelineDescription()
        {
            VertexShader = simpleVertex,
            FragmentShader = simpleFragment,
            VertexLayout = vertexLayout,
            ColorFormat = TextureFormat.RGBA32Float
        });

        float[] vertices = 
        {
            // 0.0f, 0.0f, 0.0f,    0.0f, 0.0f,
            // 0.5f, 0.0f, 0.0f,    1.0f, 0.0f,
            // 0.5f, 0.5f, 0.0f,    1.0f, 1.0f,
            // 0.0f, 0.5f, 0.0f,    0.0f, 1.0f
            -0.25f, -0.25f, 0.0f,      0.0f, 0.0f,
            0.25f, -0.25f, 0.0f,       1.0f, 0.0f,
            0.25f, 0.25f, 0.0f,        1.0f, 1.0f,
            -0.25f, 0.25f, 0.0f,       0.0f, 1.0f
        };
        using IBuffer<float> vtxBuffer = device.CreateBuffer<float>(new BufferDescription
        {
            Size = vertices.Length,
            Usage = BufferUsage.Vertex
        });
        vtxBuffer.SetData(vertices);

        uint[] indices =
        {
            0, 1, 2, 2, 3, 0
        };
        using IBuffer<uint> idxBuffer = device.CreateBuffer<uint>(new BufferDescription
        {
            Size = indices.Length,
            Usage = BufferUsage.Index
        });
        idxBuffer.SetData(indices);

        using IBuffer<float> triangleBuffer = device.CreateBuffer(new BufferDescription
        {
            Size = 3 * 5,
            Usage = BufferUsage.Vertex
        }, [
            1.0f, 0.0f, 0.0f,   0.0f, 0.0f,
            1.0f, 0.0f, 1.0f,   1.0f, 0.0f,
            1.0f, 1.0f, 0.0f,   1.0f, 1.0f
        ]);

        using IDepthStencilState depthStencil = device.CreateDepthStencilState(new DepthStencilDescription
        {
            DepthCompareFunction = CompareFunction.Less,
            DepthTestEnabled = true,
            DepthWriteEnabled = true
        });

        var texSource = decodl.PNGDecoder.Decode("assets/grass.png");
        using ITexture texture = device.CreateTexture(new TextureDescription
        {
            Width = (uint) texSource.Width,
            Height = (uint) texSource.Height,
            Format = TextureFormat.RGBA8Unorm
        }, texSource.Bytes);
        using ISampler sampler = device.CreateSampler(new SamplerDescription
        {
            MinFilter = TextureFilter.Nearest,
            MagFilter = TextureFilter.Nearest,
            MipmapFilter = MipmapFilter.Linear
        });

        Matrix4x4 view = Matrix4x4.CreateLookAt(
            new Vector3(0.0f, 0.0f, 2.0f),
            Vector3.Zero,
            Vector3.UnitY
        );
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 3.0f,
            (float) winWidth / winHeight,
            0.1f,
            100.0f
        );
        using IBuffer<float> camBuffer = device.CreateBuffer(new BufferDescription
        {
            Size = 16,
            Usage = BufferUsage.Uniform,
        }, ToBufferArray(view * projection));

        Vector3 cameraPosition = new(0.0f, 0.0f, 2.0f);
        float cameraYaw = 0.0f;
        float cameraPitch = 0.0f;
        double previousTime = GLFW.glfwGetTime();
        GLFW.glfwSetInputMode(winHandle, GLFW.GLFW_CURSOR, GLFW.GLFW_CURSOR_DISABLED);
        GLFW.glfwGetCursorPos(winHandle, out double previousMouseX, out double previousMouseY);

        while (GLFW.glfwWindowShouldClose(winHandle) == 0)
        {
            GLFW.glfwPollEvents();
            if (GLFW.glfwGetKey(winHandle, (int) Key.Escape) == GLFW.GLFW_PRESS)
            {
                GLFW.glfwSetWindowShouldClose(winHandle, 1);
            }

            using IRenderFrame? frame = surface.AcquireFrame();
            if (frame == null) throw new RenderException("No frame could be acquired.");

            using IRenderPass pass = frame.CreateRenderPass(new RenderPassDescription
            {
                Color = new ColorAttachmentDescription { ClearColor = new Color(0, 0, 0) }
            });

            double currentTime = GLFW.glfwGetTime();
            float deltaTime = MathF.Min((float)(currentTime - previousTime), 0.1f);
            previousTime = currentTime;

            GLFW.glfwGetCursorPos(winHandle, out double mouseX, out double mouseY);
            cameraYaw += (float)(mouseX - previousMouseX) * 0.0025f;
            cameraPitch -= (float)(mouseY - previousMouseY) * 0.0025f;
            cameraPitch = Math.Clamp(cameraPitch, -MathF.PI / 2.0f + 0.01f, MathF.PI / 2.0f - 0.01f);
            previousMouseX = mouseX;
            previousMouseY = mouseY;

            Vector3 forward = new(
                MathF.Sin(cameraYaw) * MathF.Cos(cameraPitch),
                MathF.Sin(cameraPitch),
                -MathF.Cos(cameraYaw) * MathF.Cos(cameraPitch));
            Vector3 movementForward = Vector3.Normalize(new(forward.X, 0.0f, forward.Z));
            Vector3 right = new(MathF.Cos(cameraYaw), 0.0f, MathF.Sin(cameraYaw));
            if (GLFW.glfwGetKey(winHandle, GLFW.GLFW_KEY_W) == GLFW.GLFW_PRESS)
                cameraPosition += movementForward * (deltaTime * 2.0f);
            if (GLFW.glfwGetKey(winHandle, GLFW.GLFW_KEY_S) == GLFW.GLFW_PRESS)
                cameraPosition -= movementForward * (deltaTime * 2.0f);
            if (GLFW.glfwGetKey(winHandle, GLFW.GLFW_KEY_A) == GLFW.GLFW_PRESS)
                cameraPosition -= right * (deltaTime * 2.0f);
            if (GLFW.glfwGetKey(winHandle, GLFW.GLFW_KEY_D) == GLFW.GLFW_PRESS)
                cameraPosition += right * (deltaTime * 2.0f);
            if (GLFW.glfwGetKey(winHandle, GLFW.GLFW_KEY_SPACE) == GLFW.GLFW_PRESS)
                cameraPosition += Vector3.UnitY * (deltaTime * 2.0f);
            if (GLFW.glfwGetKey(winHandle, GLFW.GLFW_KEY_LEFT_SHIFT) == GLFW.GLFW_PRESS)
                cameraPosition -= Vector3.UnitY * (deltaTime * 2.0f);

            Matrix4x4 currentView = Matrix4x4.CreateLookAt(
                cameraPosition,
                cameraPosition + forward,
                Vector3.UnitY);
            camBuffer.SetData(ToBufferArray(currentView * projection));

            pass.SetRenderPipeline(renderPipeline);
            pass.SetDepthStencilState(depthStencil);
            pass.SetVertexBuffer(camBuffer, 1);
            pass.SetVertexBuffer(vtxBuffer, 0);
            pass.SetIndexBuffer(idxBuffer);
            pass.SetTexture(texture);
            pass.SetSampler(sampler);
            pass.DrawIndexed();

            pass.SetRenderPipeline(simpleRenderPipleline);
            pass.SetVertexBuffer(triangleBuffer);
            pass.Draw(0, 3);

            pass.End();

            frame.Present();
        }

        GLFW.glfwTerminate();

        /*int width = 800;
        int height = 600;

        GLFW.Load();
        RenderFactory.Create("opengl");
        
        Window window = new Window(width, height, "odl3d");
        window.BackgroundColor = new Color(0, 0, 0);
        window.Camera = new MoveableCamera(window);
        window.SetCursorCapture(true);
        window.RegisterKeyPress(Key.Escape, window.Close);
        window.RegisterKeyPress(Key.M, () => window.SetWireFrame(!window.Wireframe));

        bool metal = RenderFactory.Renderer is odl3d.Renderer.MetalOld;
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
        shader.Dispose();*/
    }

    private static float[] ToBufferArray(Matrix4x4 matrix)
    {
        return
        [
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        ];
    }
}
