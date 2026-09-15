using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class Buffer : ObjCObject
    {
        public Buffer(IntPtr handle) : base(handle) { }
    }
}
