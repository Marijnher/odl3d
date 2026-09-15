using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class Texture : ObjCObject
    {
        public Texture(IntPtr handle) : base(handle, true) { }

        public void Upload(IntPtr bytes, nuint bytesPerRow, nuint width, nuint height) =>
            Send("replaceRegion:mipmapLevel:withBytes:bytesPerRow:", bytes, bytesPerRow, width, height);
    }
}
