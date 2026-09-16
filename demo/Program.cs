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
                    device const VertexIn* vertices [[buffer(0)]]
                )
{
    VertexOut out;
    out.position = float4(vertices[vertexID].position, 1.0);
    out.texCoord = vertices[vertexID].texCoord;
    return out;
}";

    private const string SimpleMetalFragmentSource = @"#include <metal_stdlib>
using namespace metal;

fragment float4 fragment_main(VertexOut in [[stage_in]])
{
    return float4(in.texCoord, 1.0, 1.0);
}";

    public static void Main(string[] args)
    {
        using IRenderDevice device = new odl3d.Renderer.MetalAdapter.MetalRenderDevice();
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
            VertexLayout = new VertexLayoutDescription()
            {
                Buffers = [],
                Attributes = []
            },
            ColorFormat = TextureFormat.RGBA32Float
        });
        float[] vertices = 
        {
            0.0f, 0.0f, 0.0f,    0.0f, 0.0f,
            0.5f, 0.0f, 0.0f,    1.0f, 0.0f,
            0.5f, 0.5f, 0.0f,    1.0f, 1.0f,
            0.0f, 0.5f, 0.0f,    0.0f, 1.0f
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

        while (GLFW.glfwWindowShouldClose(winHandle) == 0)
        {
            GLFW.glfwPollEvents();
            if (GLFW.glfwGetKey(winHandle, (int) Key.Escape) == GLFW.GLFW_PRESS)
            {
                GLFW.glfwSetWindowShouldClose(winHandle, 1);
            }

            IRenderFrame? frame = surface.AcquireFrame();
            if (frame == null) throw new RenderException("No frame could be acquired.");

            IRenderPass pass = frame.CreateRenderPass(new RenderPassDescription
            {
                Color = new ColorAttachmentDescription { ClearColor = new Color(0, 0, 0) }
            });

            pass.SetRenderPipeline(renderPipeline);
            pass.SetVertexBuffer(vtxBuffer);
            pass.SetIndexBuffer(idxBuffer);
            pass.DrawIndexed();
            pass.End();

            frame.Present();
        }

        GLFW.glfwTerminate();

        return;

        GLFW.Load();
        Metal.Load();
        using Metal.Device mDevice = Metal.Device.Default;
        Console.WriteLine(mDevice.Name);

        GLFW.glfwWindowHint(GLFW.GLFW_RESIZABLE, GLFW.GLFW_TRUE);
        int windowWidth = 400;
        int windowHeight = 400;
        nint windowHandle = GLFW.glfwCreateWindow(windowWidth, windowHeight, "Metal", IntPtr.Zero, IntPtr.Zero);
        if (windowHandle == IntPtr.Zero)
        {
            GLFW.glfwTerminate();
            throw new Exception("Failed to create a GLFW window.");
        }

        Metal.MetalLayer layer = Metal.MetalLayer.AttachToWindow(mDevice, windowHandle);
        layer.SetDisplaySyncEnabled(false);

        using Metal.CommandQueue queue = mDevice.NewCommandQueue();
        using Metal.RenderPipelineState texturedPipeline = mDevice.CreateTexturedPipeline();
        using Metal.RenderPipelineState solidPipeline = mDevice.CreateSolidTrianglePipeline();
        using Metal.DepthStencilState depthState = mDevice.CreateDepthStencilState(
            CompareFunction.LessEqual,
            depthWriteEnabled: true);
        using Metal.Texture grassTexture = mDevice.CreateTexture(new Texture("assets/grass.png"), mipmapped: true);
        using Metal.SamplerState grassSampler = mDevice.CreateSampler(
            TextureFilter.Linear,
            TextureFilter.Linear,
            MipmapFilter.Linear,
            TextureWrap.Repeat,
            TextureWrap.Repeat,
            AnisotropicFilter.X4);
        using Metal.Texture depthTexture = mDevice.CreateDepthTexture((uint)windowWidth, (uint)windowHeight);
        Matrix4x4 initialView = Matrix4x4.CreateLookAt(
            new Vector3(0.0f, 0.0f, 2.0f),
            Vector3.Zero,
            Vector3.UnitY);
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 3.0f,
            (float)windowWidth / windowHeight,
            0.1f,
            100.0f);
        using Metal.Buffer cameraBuffer = mDevice.CreateBuffer(ToMetalMatrix(
            Matrix4x4.Identity * initialView * projection));
        using Metal.Buffer vertexBuffer1 = mDevice.CreateBuffer(
        [
             0.0f,  0.0f, 0.0f,    0.0f, 0.0f,
             0.5f,  0.0f, 0.0f,    1.0f, 0.0f,
             0.5f,  0.5f, 0.0f,    1.0f, 1.0f,
             0.0f,  0.5f, 0.0f,    0.0f, 1.0f
        ]);
        using Metal.Buffer indexBuffer1 = mDevice.CreateBuffer(
            [0, 1, 2, 2, 3, 0]
        );
        using Metal.Buffer vertexBuffer2 = mDevice.CreateBuffer(
        [
             0.0f,  0.0f, 0.0f,    0.0f, 0.0f,
            -0.5f,  0.0f, 0.0f,    1.0f, 0.0f,
            -0.5f, -0.5f, 0.0f,    1.0f, 1.0f
        ]);
        using Metal.Buffer indexBuffer2 = mDevice.CreateBuffer(
            [0, 1, 2]
        );
        Vector3 cameraPosition = new(0.0f, 0.0f, 2.0f);
        float cameraYaw = 0.0f;
        float cameraPitch = 0.0f;
        GLFW.glfwSetInputMode(windowHandle, GLFW.GLFW_CURSOR, GLFW.GLFW_CURSOR_DISABLED);
        GLFW.glfwGetCursorPos(windowHandle, out double previousMouseX, out double previousMouseY);
        double previousTime = GLFW.glfwGetTime();

        while (GLFW.glfwWindowShouldClose(windowHandle) == 0)
        {
            GLFW.glfwPollEvents();

            if (GLFW.glfwGetKey(windowHandle, GLFW.GLFW_KEY_ESCAPE) == GLFW.GLFW_PRESS)
                GLFW.glfwSetWindowShouldClose(windowHandle, GLFW.GLFW_TRUE);

            double currentTime = GLFW.glfwGetTime();
            float deltaTime = MathF.Min((float)(currentTime - previousTime), 0.1f);
            previousTime = currentTime;

            GLFW.glfwGetCursorPos(windowHandle, out double mouseX, out double mouseY);
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
            if (GLFW.glfwGetKey(windowHandle, GLFW.GLFW_KEY_W) == GLFW.GLFW_PRESS)
                cameraPosition += movementForward * (deltaTime * 2.0f);
            if (GLFW.glfwGetKey(windowHandle, GLFW.GLFW_KEY_S) == GLFW.GLFW_PRESS)
                cameraPosition -= movementForward * (deltaTime * 2.0f);
            if (GLFW.glfwGetKey(windowHandle, GLFW.GLFW_KEY_A) == GLFW.GLFW_PRESS)
                cameraPosition -= right * (deltaTime * 2.0f);
            if (GLFW.glfwGetKey(windowHandle, GLFW.GLFW_KEY_D) == GLFW.GLFW_PRESS)
                cameraPosition += right * (deltaTime * 2.0f);
            if (GLFW.glfwGetKey(windowHandle, GLFW.GLFW_KEY_SPACE) == GLFW.GLFW_PRESS)
                cameraPosition += Vector3.UnitY * (deltaTime * 2.0f);
            if (GLFW.glfwGetKey(windowHandle, GLFW.GLFW_KEY_LEFT_SHIFT) == GLFW.GLFW_PRESS)
                cameraPosition -= Vector3.UnitY * (deltaTime * 2.0f);

            Matrix4x4 currentView = Matrix4x4.CreateLookAt(
                cameraPosition,
                cameraPosition + forward,
                Vector3.UnitY);
            cameraBuffer.Update(ToMetalMatrix(currentView * projection));

            Metal.Drawable drawable = layer.NextDrawable();
            
            Metal.RenderPassDescriptor pass = mDevice.NewRenderPassDescriptor();
            Metal.RenderPassColorAttachment colAtch0 = pass.ColorAttachments[0];
            colAtch0.SetTexture(drawable.Texture);
            colAtch0.SetLoadAction(LoadAction.Clear);
            colAtch0.SetStoreAction(StoreAction.Store);
            colAtch0.SetClearColor(0, 0, 0, 1);
            Metal.RenderPassDepthAttachment depthAttachment = pass.DepthAttachment;
            depthAttachment.SetTexture(depthTexture);
            depthAttachment.SetLoadAction(LoadAction.Clear);
            depthAttachment.SetStoreAction(StoreAction.DontCare);
            depthAttachment.SetClearDepth(1.0);

            Metal.CommandBuffer cmdBuf = queue.CreateCommandBuffer();

            Metal.CommandEncoder encoder = cmdBuf.RenderCommandEncoder(pass);
            encoder.SetDepthStencilState(depthState);
            encoder.SetVertexBuffer(cameraBuffer, 1);

            encoder.SetRenderPipelineState(texturedPipeline);
            encoder.SetVertexBuffer(vertexBuffer1, 0);
            encoder.SetFragmentTexture(grassTexture);
            encoder.SetFragmentSamplerState(grassSampler);
            encoder.DrawIndexedPrimitives(indexBuffer1);

            encoder.SetRenderPipelineState(solidPipeline);
            encoder.SetVertexBuffer(vertexBuffer2, 0);
            encoder.DrawIndexedPrimitives(indexBuffer2);

            encoder.EndEncoding();

            cmdBuf.Present(drawable);
            cmdBuf.Commit();
        }

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
        shader.Dispose();
    }

    private static float[] ToMetalMatrix(Matrix4x4 matrix)
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
