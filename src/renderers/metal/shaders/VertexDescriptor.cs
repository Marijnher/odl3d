using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class VertexDescriptor : ObjCObject
    {
        private static IntPtr ClassPointer => Class("MTLVertexDescriptor");

        public VertexAttributeDescriptorArray Attributes =>
            Get<VertexAttributeDescriptorArray>("attributes");

        public VertexLayoutDescriptorArray Layouts =>
            Get<VertexLayoutDescriptorArray>("layouts");

        public VertexDescriptor(IntPtr handle) : base(handle) { }

        public static VertexDescriptor Create() =>
            new VertexDescriptor(SendRaw(ClassPointer, "new"));
    }
}