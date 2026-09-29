using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents an array of Metal render pipeline color attachments.
    /// </summary>
    public sealed class RenderPipelineColorAttachmentArray : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPipelineColorAttachmentArray"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying Objective-C object.</param>
        public RenderPipelineColorAttachmentArray(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Gets the render pipeline color attachment at the specified index.
        /// </summary>
        /// <param name="index">The index of the render pipeline color attachment to retrieve.</param>
        /// <returns>The render pipeline color attachment at the specified index.</returns>
        public RenderPipelineColorAttachment Get(int index) =>
            Send<RenderPipelineColorAttachment>("objectAtIndexedSubscript:", index);

        /// <summary>
        /// Gets the render pipeline color attachment at the specified index using the indexer syntax.
        /// </summary>
        /// <param name="index">The index of the render pipeline color attachment to retrieve.</param>
        /// <returns>The render pipeline color attachment at the specified index.</returns>
        public RenderPipelineColorAttachment this[int index] => Get(index);
    }

    /// <summary>
    /// Represents a Metal render pipeline color attachment, which defines the properties of a color attachment in a render pipeline.
    /// </summary>
    public sealed class RenderPipelineColorAttachment : ObjCObject
    {
        /// <summary>
        /// Gets or sets the pixel format of the render pipeline color attachment.
        /// </summary>
        public nuint PixelFormat
        {
            get => GetUInt32("pixelFormat");
            set => Send("setPixelFormat:", value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether blending is enabled for the render pipeline color attachment.
        /// </summary>
        public bool BlendingEnabled
        {
            get => GetUInt32("blendingEnabled") == 1;
            set => Send("setBlendingEnabled:", value ? 1 : 0);
        }

        /// <summary>
        /// Gets or sets the source RGB blend factor for the render pipeline color attachment.
        /// </summary>
        public nuint SourceRGBBlendFactor
        {
            get => GetUInt32("sourceRGBBlendFactor");
            set => Send("setSourceRGBBlendFactor:", value);
        }

        /// <summary>
        /// Gets or sets the destination RGB blend factor for the render pipeline color attachment.
        /// </summary>
        public nuint DestinationRGBBlendFactor
        {
            get => GetUInt32("destinationRGBBlendFactor");
            set => Send("setDestinationRGBBlendFactor:", value);
        }

        /// <summary>
        /// Gets or sets the RGB blend operation for the render pipeline color attachment.
        /// </summary>
        public nuint RgbBlendOperation
        {
            get => GetUInt32("rgbBlendOperation");
            set => Send("setRgbBlendOperation:", value);
        }

        /// <summary>
        /// Gets or sets the source alpha blend factor for the render pipeline color attachment.
        /// </summary>
        public nuint SourceAlphaBlendFactor
        {
            get => GetUInt32("sourceAlphaBlendFactor");
            set => Send("setSourceAlphaBlendFactor:", value);
        }

        /// <summary>
        /// Gets or sets the destination alpha blend factor for the render pipeline color attachment.
        /// </summary>
        public nuint DestinationAlphaBlendFactor
        {
            get => GetUInt32("destinationAlphaBlendFactor");
            set => Send("setDestinationAlphaBlendFactor:", value);
        }

        /// <summary>
        /// Gets or sets the alpha blend operation for the render pipeline color attachment.
        /// </summary>
        public nuint AlphaBlendOperation
        {
            get => GetUInt32("alphaBlendOperation");
            set => Send("setAlphaBlendOperation:", value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPipelineColorAttachment"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The native handle to the render pipeline color attachment.</param>
        public RenderPipelineColorAttachment(IntPtr handle) : base(handle) { }
    }
}