using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class BlitCommandEncoder : ObjCObject
    {
        public BlitCommandEncoder(IntPtr handle) : base(handle) { }

        public void GenerateMipmaps(Texture texture) =>
            Send("generateMipmapsForTexture:", texture);

        public void EndEncoding() => Send("endEncoding");
    }
}
