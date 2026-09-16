using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using odl3d;

namespace odl3d.Renderer;

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

        public string Name => GetString("name");
        public string Description => GetString("description");
        public ulong RegistryID => GetUInt64("registryID");

        public CommandQueue NewCommandQueue() =>
            new CommandQueue(GetRaw("newCommandQueue"), true);

        public RenderPassDescriptor NewRenderPassDescriptor() => RenderPassDescriptor.Create();

        public DepthStencilState CreateDepthStencilState(CompareFunction depthCompareFunction = CompareFunction.LessEqual, bool depthWriteEnabled = true)
        {
            using DepthStencilDescriptor descriptor = DepthStencilDescriptor.Create();
            descriptor.SetDepthCompareFunction(depthCompareFunction);
            descriptor.SetDepthWriteEnabled(depthWriteEnabled);

            IntPtr handle = Send("newDepthStencilStateWithDescriptor:", descriptor);
            if (handle == IntPtr.Zero)
                throw new RenderException("Metal could not create the depth stencil state.");

            return new DepthStencilState(handle);
        }

        public SamplerState CreateSampler(
            TextureFilter minFilter = TextureFilter.Nearest,
            TextureFilter magFilter = TextureFilter.Nearest,
            MipmapFilter mipmapFilter = MipmapFilter.None,
            TextureWrap wrapH = TextureWrap.Repeat,
            TextureWrap wrapV = TextureWrap.Repeat,
            AnisotropicFilter anisotropicFilter = AnisotropicFilter.None)
        {
            using SamplerDescriptor descriptor = SamplerDescriptor.Create();
            descriptor.SetMinFilter(minFilter);
            descriptor.SetMagFilter(magFilter);
            descriptor.SetMipFilter(mipmapFilter);
            descriptor.SetAddressModeS(wrapH);
            descriptor.SetAddressModeT(wrapV);
            descriptor.SetMaxAnisotropy(anisotropicFilter);

            IntPtr handle = Send("newSamplerStateWithDescriptor:", descriptor);
            if (handle == IntPtr.Zero)
                throw new RenderException("Metal could not create the texture sampler.");
            return new SamplerState(handle);
        }

        public RenderPipelineState CreateTexturedPipeline() => CreatePipeline("""
                #include <metal_stdlib>
                using namespace metal;

                struct VertexOut {
                    float4 position [[position]];
                    float2 uv;
                };

                vertex VertexOut triangle_vertex(
                    uint vertexId [[vertex_id]],
                    device const float* vertices [[buffer(0)]],
                    constant float4x4& mvp [[buffer(1)]]) {
                    VertexOut out;
                    uint offset = vertexId * 5;
                    float3 position = float3(vertices[offset], vertices[offset + 1], vertices[offset + 2]);
                    out.position = mvp * float4(position, 1.0);
                    out.uv = float2(vertices[offset + 3], vertices[offset + 4]);
                    return out;
                }

                fragment float4 triangle_fragment(VertexOut in [[stage_in]],
                    texture2d<float> colorTexture [[texture(0)]],
                    sampler textureSampler [[sampler(0)]]) {
                    return colorTexture.sample(textureSampler, in.uv);
                }
                """
            );

        public Texture CreateTexture(odl3d.Texture image, bool mipmapped = false)
        {
            using var descriptor = TextureDescriptor.Create((uint) image.Width, (uint) image.Height, mipmapped);
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
            if (mipmapped)
                GenerateMipmaps(texture);
            return texture;
        }

        public void GenerateMipmaps(Texture texture)
        {
            using CommandQueue queue = NewCommandQueue();
            CommandBuffer commandBuffer = queue.CreateCommandBuffer();
            BlitCommandEncoder encoder = commandBuffer.CreateBlitCommandEncoder();
            encoder.GenerateMipmaps(texture);
            encoder.EndEncoding();
            commandBuffer.Commit();
            commandBuffer.WaitUntilCompleted();
        }

        public Texture CreateDepthTexture(uint width, uint height)
        {
            using TextureDescriptor descriptor = TextureDescriptor.CreateDepth(width, height);
            IntPtr handle = Send("newTextureWithDescriptor:", descriptor);
            if (handle == IntPtr.Zero)
                throw new RenderException("Metal could not create the depth texture.");
            return new Texture(handle);
        }

        public RenderPipelineState CreateSolidTrianglePipeline() => CreatePipeline("""
            #include <metal_stdlib>
            using namespace metal;
            struct VertexOut { float4 position [[position]]; float2 uv; };
            vertex VertexOut triangle_vertex(uint vertexId [[vertex_id]],
                device const float* vertices [[buffer(0)]],
                constant float4x4& mvp [[buffer(1)]]) {
                VertexOut out;
                uint offset = vertexId * 5;
                float3 position = float3(vertices[offset], vertices[offset + 1],
                    vertices[offset + 2]);
                out.position = mvp * float4(position, 1.0);
                out.uv = float2(vertices[offset + 3], vertices[offset + 4]);
                return out;
            }
            fragment float4 triangle_fragment(VertexOut in [[stage_in]]) {
                return float4(0.1, 0.8, 0.3, 1.0);
            }
            """);

        public unsafe Buffer CreateBuffer<T>(T[] data) where T : unmanaged
        {
            if (data is null || data.Length == 0)
                throw new ArgumentException("The buffer data cannot be empty.", nameof(data));

            GCHandle pinned = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr handle = Send("newBufferWithBytes:length:options:",
                    pinned.AddrOfPinnedObject(), (nuint) (data.Length * sizeof(T)), 0);
                if (handle == IntPtr.Zero)
                    throw new RenderException("Metal could not create the vertex buffer.");
                return new Buffer(handle);
            }
            finally
            {
                pinned.Free();
            }
        }

        public RenderPipelineState CreatePipeline(string vertexSource, string fragmentSource, string vertexEntryPoint = "vertex_main", string fragmentEntryPoint = "fragment_main") =>
            CreatePipeline(vertexSource + "\n\n" + fragmentSource, vertexEntryPoint, fragmentEntryPoint);

        public RenderPipelineState CreatePipeline(string source, string vertexEntryPoint = "vertex_main", string fragmentEntryPoint = "fragment_main")
        {
            using Library library = CompileLibrary(source);

            using var descriptor = RenderPipelineDescriptor.Create();
            using ObjCObject vertexFunction = library.NewFunctionWithName(vertexEntryPoint);
            using ObjCObject fragmentFunction = library.NewFunctionWithName(fragmentEntryPoint);
            descriptor.SetVertexFunction(vertexFunction);
            descriptor.SetFragmentFunction(fragmentFunction);
            descriptor.SetDepthAttachmentPixelFormat(252); // Depth32Float

            var colorAttachment = descriptor.ColorAttachments[0];
            colorAttachment.SetPixelFormat(80); // BGRA8Unorm

            IntPtr pipeline = Send("newRenderPipelineStateWithDescriptor:error:", descriptor.Handle, out IntPtr error);
            ThrowIfError(pipeline, error, "Metal render pipeline creation failed");
            return new RenderPipelineState(pipeline);
        }

        public Library CompileLibrary(string source)
        {
            using NSString nsSource = NSString.Create(source);
            IntPtr handle = Send("newLibraryWithSource:options:error:", nsSource.Handle, 0, out IntPtr error);
            ThrowIfError(handle, error, "Metal shader library compilation failed");
            return new Library(handle);
        }
    }
}