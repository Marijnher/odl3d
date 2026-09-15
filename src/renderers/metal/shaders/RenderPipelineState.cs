using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class RenderPipelineState : ObjCObject
    {
        public RenderPipelineState(IntPtr handle) : base(handle, true) { }
    }
}
