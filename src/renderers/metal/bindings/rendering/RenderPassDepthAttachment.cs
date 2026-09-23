using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a depth attachment for a render pass.
    /// </summary>
    public sealed class RenderPassDepthAttachment : RenderPassAttachment
    {
        /// <summary>
        /// Gets or sets the clear depth value for the depth attachment.
        /// </summary>
        public double ClearDepth
        {
            get => GetDouble("clearDepth");
            set => Send("setClearDepth:", value);
        }   

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPassDepthAttachment"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying Objective-C object.</param>
        public RenderPassDepthAttachment(IntPtr handle) : base(handle) { }
    }
}