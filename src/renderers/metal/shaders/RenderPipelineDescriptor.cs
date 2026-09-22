using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public class RenderPipelineDescriptor : ObjCObject
    {
        public ObjCObject VertexFunction
        {
            get => Get<ObjCObject>("vertexFunction");
            set => Send("setVertexFunction:", value);
        }

        public ObjCObject FragmentFunction
        {
            get => Get<ObjCObject>("fragmentFunction");
            set => Send("setFragmentFunction:", value);
        }

        public nuint DepthAttachmentPixelFormat
        {
            get => GetUInt32("depthAttachmentPixelFormat");
            set => Send("setDepthAttachmentPixelFormat:", value);
        }

        public VertexDescriptor VertexDescriptor
        {
            get => Get<VertexDescriptor>("vertexDescriptor");
            set => Send("setVertexDescriptor:", value);
        }

        private static IntPtr ClassPointer => Class("MTLRenderPipelineDescriptor");

        public RenderPipelineColorAttachmentArray ColorAttachments =>
            Get<RenderPipelineColorAttachmentArray>("colorAttachments");

        private RenderPipelineDescriptor(IntPtr handle) : base(handle, true) { }

        public static RenderPipelineDescriptor Create() =>
            new RenderPipelineDescriptor(SendRaw(ClassPointer, "new"));
    }
}