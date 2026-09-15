using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class TextureDescriptor : ObjCObject
    {
        private static IntPtr ClassPointer => Class("MTLTextureDescriptor");

        public TextureDescriptor(IntPtr handle) : base(handle) { }

        public static TextureDescriptor Create(uint width, uint height)
        {
            IntPtr handle = SendRaw(
                ClassPointer, 
                "texture2DDescriptorWithPixelFormat:width:height:mipmapped:",
                70, width, height, 0
            );
            if (handle == IntPtr.Zero)
                throw new RenderException("Metal could not create the texture.");
            return new TextureDescriptor(handle);
        }

    }
}