using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class Texture : ObjCObject
    {
        public Texture(IntPtr handle) : base(handle, true) { }

        public void Upload(IntPtr bytes, nuint mipLevel, nuint bytesPerRow, nuint width, nuint height)
        {
            MTLRegion region = new MTLRegion
            {
                X = 0,
                Y = 0,
                Z = 0,
                Width = width,
                Height = height,
                Depth = 1
            };
            Send("replaceRegion:mipmapLevel:withBytes:bytesPerRow:", region, mipLevel, bytes, bytesPerRow);
        }

        public unsafe void Upload(byte[] bytes, nuint mipLevel, nuint bytesPerRow, nuint width, nuint height)
        {
            fixed (byte* bytePtr = bytes) Upload((nint) bytePtr, mipLevel, bytesPerRow, width, height);
        }
    }
}
