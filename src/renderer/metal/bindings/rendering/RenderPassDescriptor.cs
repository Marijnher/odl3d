using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a render pass descriptor, which encapsulates the configuration of a render pass.
    /// </summary>
    public sealed class RenderPassDescriptor : ObjCObject
    {
        /// <summary>
        /// Gets the class pointer for the underlying Objective-C MTLRenderPassDescriptor class.
        /// </summary>
        public static IntPtr ClassPointer => Class("MTLRenderPassDescriptor");

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPassDescriptor"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying Objective-C object.</param>
        public RenderPassDescriptor(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Creates a new instance of the <see cref="RenderPassDescriptor"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="RenderPassDescriptor"/> class.</returns>
        public static RenderPassDescriptor Create() =>
            new RenderPassDescriptor(SendRaw(ClassPointer, "renderPassDescriptor"));

        /// <summary>
        /// Gets the array of color attachments for the render pass.
        /// </summary>
        public RenderPassColorAttachmentArray ColorAttachments =>
            Get<RenderPassColorAttachmentArray>("colorAttachments");

        /// <summary>
        /// Gets the depth attachment for the render pass.
        /// </summary>
        public RenderPassDepthAttachment DepthAttachment =>
            Get<RenderPassDepthAttachment>("depthAttachment");
    }
}
