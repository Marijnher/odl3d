using System;
using System.Collections.Generic;
using System.Numerics;
using odl3d;
using odl3d.Renderer;
using odl3d.Renderer.MetalAdapter;

namespace odl3ddemo;

public static class ProgramGenericRenderer
{
    public static void MainGeneric(string[] args)
    {
        using IRenderDevice device = new MetalRenderDevice();
        Console.WriteLine($"Renderer: {device.Name}");

        GLFW.Load();
        GLFW.glfwWindowHint(GLFW.GLFW_RESIZABLE, GLFW.GLFW_TRUE);
        int winWidth = 400;
        int winHeight = 400;
        nint winHandle = GLFW.glfwCreateWindow(winWidth, winHeight, device.Name, IntPtr.Zero, IntPtr.Zero);
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
            Source = "demo/shaders/msl/textured_vertex.metal",
            SourceAsFilename = true,
            EntryPoint = "vertex_main"
        });
        using IShaderModule fragmentShader = device.CreateShaderModule(new ShaderModuleDescription
        {
            Stage = ShaderStage.Fragment,
            Source = "demo/shaders/msl/textured_fragment.metal",
            SourceAsFilename = true,
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
            Source = "demo/shaders/msl/simple_vertex.metal",
            SourceAsFilename = true,
            EntryPoint = "vertex_main"
        });
        using IShaderModule simpleFragment = device.CreateShaderModule(new ShaderModuleDescription
        {
            Stage = ShaderStage.Fragment,
            Source = "demo/shaders/msl/simple_fragment.metal",
            SourceAsFilename = true,
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
