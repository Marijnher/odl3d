using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class Device : ObjCObject
    {
        public Device(IntPtr handle) : base(handle) { }

        public string? Name => GetString("name");
        public string Description => GetString("description");
        public ulong RegistryID => GetUInt64("registryID");

        public CommandQueue NewCommandQueue() => Get<CommandQueue>("newCommandQueue");

        public RenderPassDescriptor NewRenderPassDescriptor() => RenderPassDescriptor.Create();

        public RenderPipelineState CreateBasicTrianglePipeline() => CreatePipeline("""
                #include <metal_stdlib>
                using namespace metal;

                vertex float4 triangle_vertex(uint vertexId [[vertex_id]]) {
                    constexpr float2 positions[] = {
                        float2(0.0, 0.75),
                        float2(-0.75, -0.75),
                        float2(0.75, -0.75)
                    };
                    return float4(positions[vertexId], 0.0, 1.0);
                }

                fragment float4 triangle_fragment() {
                    return float4(0.1, 0.8, 0.3, 1.0);
                }
                """
            );

        public RenderPipelineState CreatePipeline(string source)
        {
            Library library = CompileLibrary(source);

            var descriptor = RenderPipelineDescriptor.Create();
            descriptor.SetVertexFunction(library.NewFunctionWithName("triangle_vertex"));
            descriptor.SetFragmentFunction(library.NewFunctionWithName("triangle_fragment"));

            var colorAttachment = descriptor.ColorAttachments[0];
            colorAttachment.SetPixelFormat(80); // BGRA8Unorm

            IntPtr pipeline = Send("newRenderPipelineStateWithDescriptor:error:", descriptor.Handle, out IntPtr error);
            ThrowIfError(pipeline, error, "Metal render pipeline creation failed");
            return new RenderPipelineState(pipeline);
        }

        public Library CompileLibrary(string source)
        {
            NSString nsSource = NSString.Create(source);
            IntPtr handle = Send("newLibraryWithSource:options:error:", nsSource.Handle, 0, out IntPtr error);
            ThrowIfError(handle, error, "Metal shader library compilation failed");
            return new Library(handle);
        }
    }
}