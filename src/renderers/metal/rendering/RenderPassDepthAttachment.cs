using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class RenderPassDepthAttachment : RenderPassAttachment
    {
        public RenderPassDepthAttachment(IntPtr handle) : base(handle) { }

        public void SetClearDepth(double depth) => Send("setClearDepth:", depth);
    }
}