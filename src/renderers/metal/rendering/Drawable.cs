using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class Drawable : ObjCObject
    {
        public Drawable(IntPtr handle) : base(handle) { }
        public ObjCObject Texture => Get("texture");
    }
}
