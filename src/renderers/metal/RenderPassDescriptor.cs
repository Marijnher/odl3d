using System;

namespace odl3d.Renderers;

public static partial class MetalNew
{
    public sealed class RenderPassDescriptor : ObjCObject
    {
        public static IntPtr ClassPointer => Class("MTLRenderPassDescriptor");

        public RenderPassDescriptor(IntPtr handle) : base(handle) { }

        public static RenderPassDescriptor Create() =>
            new RenderPassDescriptor(SendRaw(ClassPointer, "renderPassDescriptor"));

        public RenderPassColorAttachmentArray ColorAttachments =>
            Get<RenderPassColorAttachmentArray>("colorAttachments");
    }

    public enum LoadAction : ulong
    {
        DontCare = 0,
        Load = 1,
        Clear = 2
    }

    public enum StoreAction : ulong
    {
        DontCare = 0,
        Store = 1
    }
}
