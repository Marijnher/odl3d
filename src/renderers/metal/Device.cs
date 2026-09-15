using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class Device : ObjCObject
    {
        public Device(IntPtr handle) : base(handle) { }

        public static Device Default
        {
            get
            {
                IntPtr device = MTLCreateSystemDefaultDevice();
                if (device == IntPtr.Zero)
                    throw new RenderException("Metal did not provide a system default device.");
                return new Device(device);
            }
        }

        public string? Name => GetString("name");
        public string Description => GetString("description");
        public ulong RegistryID => GetUInt64("registryID");

        public CommandQueue NewCommandQueue() => Get<CommandQueue>("newCommandQueue");

        public RenderPassDescriptor NewRenderPassDescriptor() => RenderPassDescriptor.Create();

        public RenderPipelineState CreateBasicTrianglePipeline() => CreatePipeline("""
                #include <metal_stdlib>
                using namespace metal;

                struct VertexOut {
                    float4 position [[position]];
                    float2 uv;
                };

                vertex VertexOut triangle_vertex(
                    uint vertexId [[vertex_id]],
                    device const float* vertices [[buffer(0)]]) {
                    VertexOut out;
                    uint offset = vertexId * 5;
                    out.position = float4(
                        vertices[offset],
                        vertices[offset + 1],
                        vertices[offset + 2],
                        1.0);
                    out.uv = float2(vertices[offset + 3], vertices[offset + 4]);
                    return out;
                }

                fragment float4 triangle_fragment(VertexOut in [[stage_in]]) {
                    return float4(in.uv, 1.0, 1.0);
                }
                """
            );

        public Buffer CreateBuffer(float[] data)
        {
            if (data is null || data.Length == 0)
                throw new ArgumentException("The buffer data cannot be empty.", nameof(data));

            GCHandle pinned = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                // TODO: Investigate memory leak potential
                IntPtr handle = Send("newBufferWithBytes:length:options:",
                    pinned.AddrOfPinnedObject(), (nuint) (data.Length * sizeof(float)), 0);
                if (handle == IntPtr.Zero)
                    throw new RenderException("Metal could not create the vertex buffer.");
                return new Buffer(handle);
            }
            finally
            {
                pinned.Free();
            }
        }

        public Buffer CreateIndexBuffer(uint[] data)
        {
            if (data is null || data.Length == 0)
                throw new ArgumentException("The index data cannot be empty.", nameof(data));

            GCHandle pinned = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr handle = Send("newBufferWithBytes:length:options:",
                    pinned.AddrOfPinnedObject(), (nuint)(data.Length * sizeof(uint)), 0);
                if (handle == IntPtr.Zero)
                    throw new RenderException("Metal could not create the index buffer.");
                return new Buffer(handle);
            }
            finally
            {
                pinned.Free();
            }
        }

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