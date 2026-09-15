using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class Texture : ObjCObject
    {
        public Texture(IntPtr handle) : base(handle) { }

        public void Upload(IntPtr bytes, nuint bytesPerRow, nuint width, nuint height) =>
            SendRaw(Handle, "replaceRegion:mipmapLevel:withBytes:bytesPerRow:", bytes, bytesPerRow, width, height);
    }
}
