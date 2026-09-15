using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public class RenderPipelineDescriptor : ObjCObject
    {
        private static IntPtr ClassPointer => Class("MTLRenderPipelineDescriptor");

        public RenderPipelineColorAttachmentArray ColorAttachments =>
            Get<RenderPipelineColorAttachmentArray>("colorAttachments");

        public RenderPipelineDescriptor(IntPtr handle) : base(handle) { }

        public static RenderPipelineDescriptor Create() =>
            new RenderPipelineDescriptor(SendRaw(ClassPointer, "new"));

        public void SetVertexFunction(ObjCObject vertexFunction) =>
            Send("setVertexFunction:", vertexFunction);

        public void SetFragmentFunction(ObjCObject fragmentFunction) =>
            Send("setFragmentFunction:", fragmentFunction);
    }
}