using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class RenderPassColorAttachmentArray : ObjCObject
    {
        public RenderPassColorAttachmentArray(IntPtr handle) : base(handle) { }

        public RenderPassColorAttachment Get(int index) =>
            Send<RenderPassColorAttachment>("objectAtIndexedSubscript:", index);

        public RenderPassColorAttachment this[int index] => Get(index);
    }

    public sealed class RenderPassColorAttachment : RenderPassAttachment
    {
        public RenderPassColorAttachment(IntPtr handle) : base(handle) { }

        public void SetClearColor(double red, double green, double blue, double alpha) =>
            Send("setClearColor:", red, green, blue, alpha);
    }
}