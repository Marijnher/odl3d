using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class RenderPipelineColorAttachmentArray : ObjCObject
    {
        public RenderPipelineColorAttachmentArray(IntPtr handle) : base(handle) { }

        public RenderPipelineColorAttachment Get(int index) =>
            Send<RenderPipelineColorAttachment>("objectAtIndexedSubscript:", index);

        public RenderPipelineColorAttachment this[int index] => Get(index);
    }

    public sealed class RenderPipelineColorAttachment : ObjCObject
    {
        public RenderPipelineColorAttachment(IntPtr handle) : base(handle) { }

        public void SetPixelFormat(uint pixelFormat) =>
            Send("setPixelFormat:", pixelFormat);
    }
}