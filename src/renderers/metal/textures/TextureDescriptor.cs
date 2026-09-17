using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class TextureDescriptor : ObjCObject
    {
        private static IntPtr ClassPointer => Class("MTLTextureDescriptor");

        private TextureDescriptor(IntPtr handle) : base(handle) { }

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