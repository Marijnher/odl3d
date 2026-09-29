using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal texture descriptor, which encapsulates the configuration of a texture.
    /// </summary>
    public sealed class TextureDescriptor : ObjCObject
    {
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
        /// <returns>A new <see cref="TextureDescriptor"/> instance.</returns>
        public static TextureDescriptor Create(uint width, uint height, TextureFormat textureFormat = TextureFormat.RGBA8Unorm)
        {
            nuint pixelFormat = GetPixelFormat(textureFormat);
            IntPtr handle = SendRaw(
                ClassPointer,
                "texture2DDescriptorWithPixelFormat:width:height:mipmapped:",
                pixelFormat, width, height, 1
            );
            if (handle == IntPtr.Zero)
                throw new RenderException("Metal could not create the texture.");
            return new TextureDescriptor(handle);
        }
    }
}