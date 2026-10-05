using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal texture descriptor, which encapsulates the configuration of a texture.
    /// </summary>
    public sealed class TextureDescriptor : ObjCObject
    {
        public const nuint UsageShaderRead = 0x0001;
        public const nuint UsageRenderTarget = 0x0004;
        public const nuint StorageModeManaged = 1;
        public const nuint StorageModePrivate = 2;

        /// <summary>
        /// Sets the MTLTextureUsage flags of the texture.
        /// </summary>
        public nuint Usage
        {
            set => Send("setUsage:", value);
        }

        /// <summary>
        /// Sets the MTLStorageMode of the texture.
        /// </summary>
        public nuint StorageMode
        {
            set => Send("setStorageMode:", value);
        }

        /// <summary>
        /// Gets the class pointer for the Metal texture descriptor.
        /// </summary>
        private static IntPtr ClassPointer => Class("MTLTextureDescriptor");

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureDescriptor"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Metal texture descriptor object.</param>
        public TextureDescriptor(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Creates a new instance of the <see cref="TextureDescriptor"/> class with the specified width, height, and texture format.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="textureFormat">The format of the texture.</param>
        /// <param name="mipmapped">Whether the texture has a full mipmap chain.</param>
        /// <returns>A new <see cref="TextureDescriptor"/> instance.</returns>
        public static TextureDescriptor Create(uint width, uint height, TextureFormat textureFormat = TextureFormat.RGBA8Unorm, bool mipmapped = true)
        {
            nuint pixelFormat = GetPixelFormat(textureFormat);
            IntPtr handle = SendRaw(
                ClassPointer,
                "texture2DDescriptorWithPixelFormat:width:height:mipmapped:",
                pixelFormat, width, height, (byte) (mipmapped ? 1 : 0)
            );
            if (handle == IntPtr.Zero)
                throw new RenderException("Metal could not create the texture.");
            return new TextureDescriptor(handle);
        }
    }
}