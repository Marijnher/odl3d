using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using odl3d;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal device, providing access to its properties and methods for creating Metal resources such as command queues, textures, and buffers.
    /// </summary>
    public sealed class Device : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class with the specified handle and ownership flag.
        /// </summary>
        /// <param name="handle">The handle to the native Metal device object.</param>
        /// <param name="ownsNativeObject">A value indicating whether the instance owns the native object.</param>
        public Device(IntPtr handle, bool ownsNativeObject = false) : base(handle, ownsNativeObject) { }

        /// <summary>
        /// Gets the default Metal device.
        /// </summary>
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

        /// <summary>
        /// Gets the name of the Metal device.
        /// </summary>
        public string Name => GetString("name");

        /// <summary>
        /// Gets the description of the Metal device.
        /// </summary>
        public string Description => GetString("description");

        /// <summary>
        /// Gets the registry ID of the Metal device.
        /// </summary>
        public ulong RegistryID => GetUInt64("registryID");

        /// <summary>
        /// Creates a new command queue for the Metal device.
        /// </summary>
        /// <returns>A new <see cref="CommandQueue"/> instance.</returns>
        public CommandQueue NewCommandQueue() =>
            new CommandQueue(GetRaw("newCommandQueue"), true);

        /// <summary>
        /// Creates a new render pass descriptor for the Metal device.
        /// </summary>
        /// <returns>A new <see cref="RenderPassDescriptor"/> instance.</returns>
        public RenderPassDescriptor NewRenderPassDescriptor() => RenderPassDescriptor.Create();

        /// <summary>
        /// Creates a new depth stencil state for the Metal device.
        /// </summary>
        /// <param name="depthCompareFunction">The depth compare function to use.</param>
        /// <param name="depthWriteEnabled">A value indicating whether depth writing is enabled.</param>
        /// <returns>A new <see cref="DepthStencilState"/> instance.</returns>
        public DepthStencilState CreateDepthStencilState(CompareFunction depthCompareFunction = CompareFunction.LessEqual, bool depthWriteEnabled = true)
        {
            using DepthStencilDescriptor descriptor = DepthStencilDescriptor.Create();
            descriptor.DepthCompareFunction = depthCompareFunction;
            descriptor.DepthWriteEnabled = depthWriteEnabled;

            IntPtr handle = Send("newDepthStencilStateWithDescriptor:", descriptor);
            if (handle == IntPtr.Zero)
                throw new RenderException("Metal could not create the depth stencil state.");

            return new DepthStencilState(handle);
        }

        /// <summary>
        /// Creates a new sampler state for the Metal device.
        /// </summary>
        /// <param name="minFilter">The minification filter to use.</param>
        /// <param name="magFilter">The magnification filter to use.</param>
        /// <param name="mipmapFilter">The mipmap filter to use.</param>
        /// <param name="wrapU">The wrap mode for the U texture coordinate.</param>
        /// <param name="wrapV">The wrap mode for the V texture coordinate.</param>
        /// <param name="wrapW">The wrap mode for the W texture coordinate.</param>
        /// <param name="anisotropicFilter">The anisotropic filter to use.</param>
        /// <returns>A new <see cref="SamplerState"/> instance.</returns>
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
            descriptor.MinFilter = GetFilter(minFilter);
            descriptor.MagFilter = GetFilter(magFilter);
            descriptor.MipFilter = GetMipmapFilter(mipmapFilter);
            descriptor.AddressModeS = GetWrap(wrapU);
            descriptor.AddressModeT = GetWrap(wrapV);
            descriptor.AddressModeR = GetWrap(wrapW);
            descriptor.MaxAnisotropy = GetAnisotropicFilter(anisotropicFilter);

            IntPtr handle = Send("newSamplerStateWithDescriptor:", descriptor);
            if (handle == IntPtr.Zero)
                throw new RenderException("Metal could not create the texture sampler.");
            return new SamplerState(handle);
        }

        /// <summary>
        /// Creates a new texture for the Metal device.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="textureFormat">The format of the texture.</param>
        /// <returns>A new <see cref="Texture"/> instance.</returns>
        public Texture CreateTexture(uint width, uint height, TextureFormat textureFormat = TextureFormat.RGBA8Unorm)
        {
            using var descriptor = TextureDescriptor.Create(width, height, textureFormat);
            Texture texture = Send<Texture>("newTextureWithDescriptor:", descriptor);
            return texture;
        }

        /// <summary>
        /// Creates a new texture for the Metal device and uploads the specified data.
        /// </summary>
        /// <param name="data">The texture data to upload.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="textureFormat">The format of the texture.</param>
        /// <returns>A new <see cref="Texture"/> instance with the uploaded data.</returns>
        public Texture CreateTexture(byte[] data, uint width, uint height, TextureFormat textureFormat = TextureFormat.RGBA8Unorm)
        {
            Texture tex = CreateTexture(width, height, textureFormat);
            tex.Upload(data, 0, width * 4, width, height);
            return tex;
        }

        /// <summary>
        /// Generates mipmaps for the specified texture using the given command queue.
        /// </summary>
        /// <param name="queue">The command queue to use for generating mipmaps.</param>
        /// <param name="texture">The texture for which to generate mipmaps.</param>
        public void GenerateMipmaps(CommandQueue queue, Texture texture)
        {
            using CommandBuffer commandBuffer = queue.CreateCommandBuffer();
            using BlitCommandEncoder encoder = commandBuffer.CreateBlitCommandEncoder();
            encoder.GenerateMipmaps(texture);
            encoder.EndEncoding();
            commandBuffer.Commit();
        }

        /// <summary>
        /// Creates a new buffer for the Metal device and uploads the specified data.
        /// </summary>
        /// <typeparam name="T">The type of the buffer elements.</typeparam>
        /// <param name="data">The buffer data to upload.</param>
        /// <returns>A new <see cref="Buffer"/> instance with the uploaded data.</returns>
        /// <exception cref="RenderException">Thrown if the buffer data is empty or if Metal could not create the buffer.</exception>
        public unsafe Buffer CreateBuffer<T>(T[] data) where T : unmanaged
        {
            if (data is null || data.Length == 0)
                throw new RenderException("The buffer data cannot be empty.");

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

        /// <summary>
        /// Creates a new render pipeline state for the Metal device using the specified vertex and fragment shaders, entry points, and blend description.
        /// </summary>
        /// <param name="vertexDescriptor">The vertex descriptor describing the layout of vertex data.</param>
        /// <param name="vertexSource">The source code of the vertex shader.</param>
        /// <param name="fragmentSource">The source code of the fragment shader.</param>
        /// <param name="vertexEntryPoint">The entry point function name for the vertex shader.</param>
        /// <param name="fragmentEntryPoint">The entry point function name for the fragment shader.</param>
        /// <param name="blendDescription">The blend description for the render pipeline.</param>
        /// <returns>A new <see cref="RenderPipelineState"/> instance representing the created pipeline.</returns>
        public RenderPipelineState CreatePipeline(
                VertexDescriptor vertexDescriptor, 
                string vertexSource,
                string fragmentSource,
                string vertexEntryPoint,
                string fragmentEntryPoint,
                BlendDescription blendDescription) =>
            CreatePipeline(vertexDescriptor, vertexSource + "\n\n" + fragmentSource, vertexEntryPoint, fragmentEntryPoint, blendDescription);

        /// <summary>
        /// Creates a new render pipeline state for the Metal device using the specified combined shader source, entry points, and blend description.
        /// </summary>
        /// <param name="vertexDescriptor">The vertex descriptor describing the layout of vertex data.</param>
        /// <param name="source">The combined source code of the vertex and fragment shaders.</param>
        /// <param name="vertexEntryPoint">The entry point function name for the vertex shader.</param>
        /// <param name="fragmentEntryPoint">The entry point function name for the fragment shader.</param>
        /// <param name="blendDescription">The blend description for the render pipeline.</param>
        /// <returns>A new <see cref="RenderPipelineState"/> instance representing the created pipeline.</returns>
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
            descriptor.VertexFunction = vertexFunction;
            descriptor.FragmentFunction = fragmentFunction;
            descriptor.DepthAttachmentPixelFormat = GetPixelFormat(TextureFormat.Depth32Float);
            descriptor.VertexDescriptor = vertexDescriptor;

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

        /// <summary>
        /// Compiles a Metal shader library from the given source code.
        /// </summary>
        /// <param name="source">The source code of the Metal shader library.</param>
        /// <returns>A new <see cref="Library"/> instance representing the compiled shader library.</returns>
        public Library CompileLibrary(string source)
        {
            using NSString nsSource = NSString.Create(source);
            IntPtr handle = Send("newLibraryWithSource:options:error:", nsSource.Handle, 0, out IntPtr error);
            ThrowIfError(handle, error, "Metal shader library compilation failed");
            return new Library(handle);
        }
    }
}