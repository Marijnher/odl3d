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
            TextureFilter minFilter,
            TextureFilter magFilter,
            MipmapFilter mipmapFilter,
            TextureWrap wrapU,
            TextureWrap wrapV,
            TextureWrap wrapW,
            AnisotropicFilter anisotropicFilter)
        {
            using SamplerDescriptor descriptor = SamplerDescriptor.Create();
            descriptor.SetMinFilter(GetFilter(minFilter));
            descriptor.SetMagFilter(GetFilter(magFilter));
            descriptor.SetMipFilter(GetMipmapFilter(mipmapFilter));
            descriptor.SetAddressModeS(GetWrap(wrapU));
            descriptor.SetAddressModeT(GetWrap(wrapV));
            descriptor.SetAddressModeR(GetWrap(wrapW));
            descriptor.SetMaxAnisotropy(GetAnisotropicFilter(anisotropicFilter));

            IntPtr handle = Send("newSamplerStateWithDescriptor:", descriptor);
            if (handle == IntPtr.Zero)
                throw new RenderException("Metal could not create the texture sampler.");
            return new SamplerState(handle);
        }

        public Texture CreateTexture(uint width, uint height, TextureFormat textureFormat = TextureFormat.RGBA8Unorm)
        {
            using var descriptor = TextureDescriptor.Create(width, height, textureFormat);
            Texture texture = Send<Texture>("newTextureWithDescriptor:", descriptor);
            return texture;
        }

        public unsafe Texture CreateTexture(byte[] data, uint width, uint height, TextureFormat textureFormat = TextureFormat.RGBA8Unorm)
        {
            Texture tex = CreateTexture(width, height, textureFormat);
            tex.Upload(data, 0, width * 4, width, height);
            return tex;
        }

        public void GenerateMipmaps(CommandQueue queue, Texture texture)
        {
            CommandBuffer commandBuffer = queue.CreateCommandBuffer();
            BlitCommandEncoder encoder = commandBuffer.CreateBlitCommandEncoder();
            encoder.GenerateMipmaps(texture);
            encoder.EndEncoding();
            commandBuffer.Commit();
        }

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

        public RenderPipelineState CreatePipeline(
                VertexDescriptor vertexDescriptor, 
                string vertexSource,
                string fragmentSource,
                string vertexEntryPoint,
                string fragmentEntryPoint,
                BlendDescription blendDescription) =>
            CreatePipeline(vertexDescriptor, vertexSource + "\n\n" + fragmentSource, vertexEntryPoint, fragmentEntryPoint, blendDescription);

        public RenderPipelineState CreatePipeline(
                VertexDescriptor vertexDescriptor,
                string source,
                string vertexEntryPoint,
                string fragmentEntryPoint,
                BlendDescription blendDescription)
        {
            using Library library = CompileLibrary(source);

            using RenderPipelineDescriptor descriptor = RenderPipelineDescriptor.Create();
            using ObjCObject vertexFunction = library.NewFunctionWithName(vertexEntryPoint);
            using ObjCObject fragmentFunction = library.NewFunctionWithName(fragmentEntryPoint);
            descriptor.SetVertexFunction(vertexFunction);
            descriptor.SetFragmentFunction(fragmentFunction);
            descriptor.SetDepthAttachmentPixelFormat(GetPixelFormat(TextureFormat.Depth32Float));
            descriptor.SetVertexDescriptor(vertexDescriptor);

            var colorAttachment = descriptor.ColorAttachments[0];
            colorAttachment.PixelFormat = GetPixelFormat(TextureFormat.BGRA8Unorm);
            colorAttachment.BlendingEnabled = blendDescription.Enabled;
            colorAttachment.SourceRGBBlendFactor = GetBlendFactor(blendDescription.SourceColor);
            colorAttachment.DestinationRGBBlendFactor = GetBlendFactor(blendDescription.DestinationColor);
            colorAttachment.RgbBlendOperation = GetColorOperation(blendDescription.ColorOperation);
            colorAttachment.SourceAlphaBlendFactor = GetBlendFactor(blendDescription.SourceAlpha);
            colorAttachment.DestinationAlphaBlendFactor = GetBlendFactor(blendDescription.DestinationAlpha);
            colorAttachment.AlphaBlendOperation = GetColorOperation(blendDescription.AlphaOperation);

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