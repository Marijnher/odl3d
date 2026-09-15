using System;

namespace odl3d.Renderers;

public static partial class MetalNew
{
    public sealed class Drawable : ObjCObject
    {
        public Drawable(IntPtr handle) : base(handle) { }
        public ObjCObject Texture => Get("texture");
    }
}
