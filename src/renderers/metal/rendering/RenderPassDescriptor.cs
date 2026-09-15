using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class RenderPassDescriptor : ObjCObject
    {
        public static IntPtr ClassPointer => Class("MTLRenderPassDescriptor");

        public RenderPassDescriptor(IntPtr handle) : base(handle) { }

        public static RenderPassDescriptor Create() =>
            new RenderPassDescriptor(SendRaw(ClassPointer, "renderPassDescriptor"));

        public RenderPassColorAttachmentArray ColorAttachments =>
            Get<RenderPassColorAttachmentArray>("colorAttachments");

        public RenderPassDepthAttachment DepthAttachment =>
            Get<RenderPassDepthAttachment>("depthAttachment");
    }
}
