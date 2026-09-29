using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents an array of color attachments for a render pass.
    /// </summary>
    public sealed class RenderPassColorAttachmentArray : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPassColorAttachmentArray"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying Objective-C object.</param>
        public RenderPassColorAttachmentArray(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Gets the color attachment at the specified index.
        /// </summary>
        /// <param name="index">The index of the color attachment to retrieve.</param>
        /// <returns>The color attachment at the specified index.</returns>
        public RenderPassColorAttachment Get(int index) =>
            Send<RenderPassColorAttachment>("objectAtIndexedSubscript:", index);

        /// <summary>
        /// Gets the color attachment at the specified index using the indexer syntax.
        /// </summary>
        /// <param name="index">The index of the color attachment to retrieve.</param>
        /// <returns>The color attachment at the specified index.</returns>
        public RenderPassColorAttachment this[int index] => Get(index);
    }

    /// <summary>
    /// Represents a color attachment for a render pass.
    /// </summary>
    public sealed class RenderPassColorAttachment : RenderPassAttachment
    {
        public RenderPassColorAttachment(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Sets the clear color for the color attachment.
        /// </summary>
        /// <param name="red">The red component of the clear color.</param>
        /// <param name="green">The green component of the clear color.</param>
        /// <param name="blue">The blue component of the clear color.</param>
        /// <param name="alpha">The alpha component of the clear color.</param>
        public void SetClearColor(double red, double green, double blue, double alpha) =>
            Send("setClearColor:", red, green, blue, alpha);

        /// <summary>
        /// Sets the clear color for the color attachment using a <see cref="Color"/> object.
        /// </summary>
        /// <param name="color">The color to set as the clear color.</param>
        public void SetClearColor(Color color) =>
            SetClearColor(color.R / 255d, color.G / 255d, color.B / 255d, color.A / 255d);
    }
}