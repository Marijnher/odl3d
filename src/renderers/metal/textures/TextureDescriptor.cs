using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class TextureDescriptor : ObjCObject
    {
        private static IntPtr ClassPointer => Class("MTLTextureDescriptor");

        private TextureDescriptor(IntPtr handle) : base(handle) { }

        public static TextureDescriptor Create(uint width, uint height, bool mipmapped = false)
        {
            IntPtr handle = SendRaw(
                ClassPointer, 
                "texture2DDescriptorWithPixelFormat:width:height:mipmapped:",
                70, width, height, mipmapped ? (byte)1 : (byte)0
            );
            if (handle == IntPtr.Zero)
                throw new RenderException("Metal could not create the texture.");
            return new TextureDescriptor(handle);
        }

        public static TextureDescriptor CreateDepth(uint width, uint height)
        {
            IntPtr handle = SendRaw(
                ClassPointer,
                "texture2DDescriptorWithPixelFormat:width:height:mipmapped:",
                252, width, height, 0
            );
            if (handle == IntPtr.Zero)
                throw new RenderException("Metal could not create the depth texture.");
            return new TextureDescriptor(handle);
        }

    }
}