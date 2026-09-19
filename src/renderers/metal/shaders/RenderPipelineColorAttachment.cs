using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class RenderPipelineColorAttachmentArray : ObjCObject
    {
        public RenderPipelineColorAttachmentArray(IntPtr handle) : base(handle) { }

        public RenderPipelineColorAttachment Get(int index) =>
            Send<RenderPipelineColorAttachment>("objectAtIndexedSubscript:", index);

        public RenderPipelineColorAttachment this[int index] => Get(index);
    }

    public sealed class RenderPipelineColorAttachment : ObjCObject
    {
        public nuint PixelFormat
        {
            get => GetUInt32("pixelFormat");
            set => Send("setPixelFormat:", value);
        }

        public bool BlendingEnabled
        {
            get => GetUInt32("blendingEnabled") == 1;
            set => Send("setBlendingEnabled:", value ? 1 : 0);
        }

        public nuint SourceRGBBlendFactor
        {
            get => GetUInt32("sourceRGBBlendFactor");
            set => Send("setSourceRGBBlendFactor:", value);
        }

        public nuint DestinationRGBBlendFactor
        {
            get => GetUInt32("destinationRGBBlendFactor");
            set => Send("setDestinationRGBBlendFactor:", value);
        }

        public nuint RgbBlendOperation
        {
            get => GetUInt32("rgbBlendOperation");
            set => Send("setRgbBlendOperation:", value);
        }

        public nuint SourceAlphaBlendFactor
        {
            get => GetUInt32("sourceAlphaBlendFactor");
            set => Send("setSourceAlphaBlendFactor:", value);
        }

        public nuint DestinationAlphaBlendFactor
        {
            get => GetUInt32("destinationAlphaBlendFactor");
            set => Send("setDestinationAlphaBlendFactor:", value);
        }

        public nuint AlphaBlendOperation
        {
            get => GetUInt32("alphaBlendOperation");
            set => Send("setAlphaBlendOperation:", value);
        }

        public RenderPipelineColorAttachment(IntPtr handle) : base(handle) { }
    }
}