using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal render pipeline descriptor, which defines the configuration of a render pipeline.
    /// </summary>
    public class RenderPipelineDescriptor : ObjCObject
    {
        /// <summary>
        /// Gets or sets the vertex function for the render pipeline descriptor.
        /// </summary>
        public ObjCObject VertexFunction
        {
            get => Get<ObjCObject>("vertexFunction");
            set => Send("setVertexFunction:", value);
        }

        /// <summary>
        /// Gets or sets the fragment function for the render pipeline descriptor.
        /// </summary>
        public ObjCObject FragmentFunction
        {
            get => Get<ObjCObject>("fragmentFunction");
            set => Send("setFragmentFunction:", value);
        }

        /// <summary>
        /// Gets or sets the pixel format of the depth attachment for the render pipeline descriptor.
        /// </summary>
        public nuint DepthAttachmentPixelFormat
        {
            get => GetUInt32("depthAttachmentPixelFormat");
            set => Send("setDepthAttachmentPixelFormat:", value);
        }

        /// <summary>
        /// Gets or sets the vertex descriptor for the render pipeline descriptor.
        /// </summary>
        public VertexDescriptor VertexDescriptor
        {
            get => Get<VertexDescriptor>("vertexDescriptor");
            set => Send("setVertexDescriptor:", value);
        }

        /// <summary>
        /// Gets the array of color attachments for the render pipeline descriptor.
        /// </summary>
        private static IntPtr ClassPointer => Class("MTLRenderPipelineDescriptor");

        /// <summary>
        /// Gets the array of color attachments for the render pipeline descriptor.
        /// </summary>
        public RenderPipelineColorAttachmentArray ColorAttachments =>
            Get<RenderPipelineColorAttachmentArray>("colorAttachments");

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPipelineDescriptor"/> class with the specified handle.
        /// </summary>
        public RenderPipelineDescriptor(IntPtr handle) : base(handle, true) { }

        /// <summary>
        /// Creates a new instance of the <see cref="RenderPipelineDescriptor"/> class.
        /// </summary>
        public static RenderPipelineDescriptor Create() =>
            new RenderPipelineDescriptor(SendRaw(ClassPointer, "new"));
    }
}