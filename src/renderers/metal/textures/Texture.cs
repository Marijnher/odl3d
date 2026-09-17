using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class Texture : ObjCObject
    {
        public Texture(IntPtr handle) : base(handle, true) { }

        public void Upload(IntPtr bytes, int mipLevel, nuint bytesPerRow, nuint width, nuint height) =>
            Send("replaceRegion:mipmapLevel:withBytes:bytesPerRow:", mipLevel, bytes, bytesPerRow, width, height);

        public unsafe void Upload(byte[] bytes, int mipLevel, nuint bytesPerRow, nuint width, nuint height)
        {
            fixed (byte* bytePtr = bytes) Upload((nint) bytePtr, mipLevel, bytesPerRow, width, height);
        }
    }
}
