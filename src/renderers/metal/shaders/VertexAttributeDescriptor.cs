using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class VertexAttributeDescriptorArray : ObjCObject
    {
        public VertexAttributeDescriptorArray(IntPtr handle) : base(handle) { }

        public VertexAttributeDescriptor Get(int index) =>
            Send<VertexAttributeDescriptor>("objectAtIndexedSubscript:", index);

        public VertexAttributeDescriptor this[int index] => Get(index);
    }

    public sealed class VertexAttributeDescriptor : ObjCObject
    {
        public VertexFormat Format
        {
            get => GetUInt32("format") switch
            {
                28 => VertexFormat.Float1,
                29 => VertexFormat.Float2,
                30 => VertexFormat.Float3,
                31 => VertexFormat.Float4,

                25 => VertexFormat.Half2,
                27 => VertexFormat.Half4,

                32 => VertexFormat.Int1,
                33 => VertexFormat.Int2,
                34 => VertexFormat.Int3,
                35 => VertexFormat.Int4,

                36 => VertexFormat.UInt1,
                37 => VertexFormat.UInt2,
                38 => VertexFormat.UInt3,
                39 => VertexFormat.UInt4,
                uint other => (VertexFormat) other
            };
            set => Send("setFormat:", value switch
            {
                VertexFormat.Float1 => 28,
                VertexFormat.Float2 => 29,
                VertexFormat.Float3 => 30,
                VertexFormat.Float4 => 31,

                VertexFormat.Half2 => 25,
                VertexFormat.Half4 => 27,

                VertexFormat.Int1 => 32,
                VertexFormat.Int2 => 33,
                VertexFormat.Int3 => 34,
                VertexFormat.Int4 => 35,

                VertexFormat.UInt1 => 36,
                VertexFormat.UInt2 => 37,
                VertexFormat.UInt3 => 38,
                VertexFormat.UInt4 => 39,
                _ => throw new RenderException($"Unsupported vertex format '{value}'.")
            });
        }

        public int Offset
        {
            get => GetInt32("offset");
            set => Send("setOffset:", value);
        }

        public uint BufferIndex
        {
            get => GetUInt32("bufferIndex");
            set => Send("setBufferIndex:", value);
        }

        public VertexAttributeDescriptor(IntPtr handle) : base(handle) { }
    }
}