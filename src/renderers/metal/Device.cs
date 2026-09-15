using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class Device : ObjCObject
    {
        public Device(IntPtr handle, bool ownsNativeObject = false) : base(handle, ownsNativeObject) { }

        public static Device Default
        {
            get
            {
                IntPtr device = MTLCreateSystemDefaultDevice();
                if (device == IntPtr.Zero)
                    throw new RenderException("Metal did not provide a system default device.");
                return new Device(device, true);
            }
        }

        public string? Name => GetString("name");
        public string Description => GetString("description");
        public ulong RegistryID => GetUInt64("registryID");

        public CommandQueue NewCommandQueue() =>
            new CommandQueue(GetRaw("newCommandQueue"), true);

        public RenderPassDescriptor NewRenderPassDescriptor() => RenderPassDescriptor.Create();

        public RenderPipelineState CreateTexturedPipeline() => CreatePipeline("""
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

                fragment float4 triangle_fragment(VertexOut in [[stage_in]],
                    texture2d<float> colorTexture [[texture(0)]]) {
                    constexpr sampler samplerState(filter::linear, address::repeat);
                    return colorTexture.sample(samplerState, in.uv);
                }
                """
            );

        public Texture CreateTexture(odl3d.Texture image)
        {
            var descriptor = TextureDescriptor.Create((uint) image.Width, (uint) image.Height);
            Texture texture = Send<Texture>("newTextureWithDescriptor:", descriptor);

            GCHandle pinned = GCHandle.Alloc(image.Pixels, GCHandleType.Pinned);
            try
            {
                texture.Upload(pinned.AddrOfPinnedObject(), (nuint) (image.Width * 4), (uint) image.Width, (uint) image.Height);
            }
            finally
            {
                pinned.Free();
            }
            return texture;
        }

        public RenderPipelineState CreateSolidTrianglePipeline() => CreatePipeline("""
            #include <metal_stdlib>
            using namespace metal;
            struct VertexOut { float4 position [[position]]; float2 uv; };
            vertex VertexOut triangle_vertex(uint vertexId [[vertex_id]],
                device const float* vertices [[buffer(0)]]) {
                VertexOut out;
                uint offset = vertexId * 5;
                out.position = float4(vertices[offset], vertices[offset + 1],
                    vertices[offset + 2], 1.0);
                out.uv = float2(vertices[offset + 3], vertices[offset + 4]);
                return out;
            }
            fragment float4 triangle_fragment(VertexOut in [[stage_in]]) {
                return float4(0.1, 0.8, 0.3, 1.0);
            }
            """);

        public Buffer CreateBuffer(float[] data)
        {
            if (data is null || data.Length == 0)
                throw new ArgumentException("The buffer data cannot be empty.", nameof(data));

            GCHandle pinned = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
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
            using Library library = CompileLibrary(source);

            using var descriptor = RenderPipelineDescriptor.Create();
            using ObjCObject vertexFunction = library.NewFunctionWithName("triangle_vertex");
            using ObjCObject fragmentFunction = library.NewFunctionWithName("triangle_fragment");
            descriptor.SetVertexFunction(vertexFunction);
            descriptor.SetFragmentFunction(fragmentFunction);

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