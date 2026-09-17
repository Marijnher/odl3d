using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class TextureDescriptor : ObjCObject
    {
        private static IntPtr ClassPointer => Class("MTLTextureDescriptor");

        private TextureDescriptor(IntPtr handle) : base(handle) { }

        public static TextureDescriptor Create(uint width, uint height) =>
            Create(width, height, 70);

        public static TextureDescriptor CreateDepth(uint width, uint height) =>
            Create(width, height, 252);

        private static TextureDescriptor Create(uint width, uint height, nuint pixelFormat)
        {
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