using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public class RenderPipelineDescriptor : ObjCObject
    {
        private static IntPtr ClassPointer => Class("MTLRenderPipelineDescriptor");

        public RenderPipelineColorAttachmentArray ColorAttachments =>
            Get<RenderPipelineColorAttachmentArray>("colorAttachments");

        private RenderPipelineDescriptor(IntPtr handle) : base(handle, true) { }

        public static RenderPipelineDescriptor Create() =>
            new RenderPipelineDescriptor(SendRaw(ClassPointer, "new"));

        public void SetVertexFunction(ObjCObject vertexFunction) =>
            Send("setVertexFunction:", vertexFunction);

        public void SetFragmentFunction(ObjCObject fragmentFunction) =>
            Send("setFragmentFunction:", fragmentFunction);

        public void SetDepthAttachmentPixelFormat(nuint pixelFormat) =>
            Send("setDepthAttachmentPixelFormat:", pixelFormat);

        public void SetVertexDescriptor(VertexDescriptor vertexDescriptor) =>
            Send("setVertexDescriptor:", vertexDescriptor);
    }
}