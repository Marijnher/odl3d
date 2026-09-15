using System;

namespace odl3d.Renderers;

public static partial class MetalNew
{
    public class CommandBuffer : ObjCObject
    {
        public CommandBuffer(IntPtr handle) : base(handle) { }
    }
}