using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal vertex attribute descriptor array, which provides access to the vertex attribute descriptors for a render pipeline.
    /// </summary>
    public sealed class VertexAttributeDescriptorArray : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VertexAttributeDescriptorArray"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Metal vertex attribute descriptor array object.</param>
        public VertexAttributeDescriptorArray(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Gets the vertex attribute descriptor at the specified index.
        /// </summary>
        /// <param name="index">The index of the vertex attribute descriptor to retrieve.</param>
        /// <returns>The vertex attribute descriptor at the specified index.</returns>
        public VertexAttributeDescriptor Get(int index) =>
            Send<VertexAttributeDescriptor>("objectAtIndexedSubscript:", index);

        /// <summary>
        /// Gets the vertex attribute descriptor at the specified index using array-like indexing.
        /// </summary>
        /// <param name="index">The index of the vertex attribute descriptor to retrieve.</param>
        /// <returns>The vertex attribute descriptor at the specified index.</returns>
        public VertexAttributeDescriptor this[int index] => Get(index);
    }

    /// <summary>
    /// Represents a Metal vertex attribute descriptor, which describes the format, offset, and buffer index of a vertex attribute.
    /// </summary>
    public sealed class VertexAttributeDescriptor : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VertexAttributeDescriptor"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Metal vertex attribute descriptor object.</param>
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

        /// <summary>
        /// Gets or sets the offset, in bytes, of the vertex attribute within the vertex buffer.
        /// </summary>
        public int Offset
        {
            get => GetInt32("offset");
            set => Send("setOffset:", value);
        }

        /// <summary>
        /// Gets or sets the index of the vertex buffer that contains the vertex attribute.
        /// </summary>
        public uint BufferIndex
        {
            get => GetUInt32("bufferIndex");
            set => Send("setBufferIndex:", value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexAttributeDescriptor"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Metal vertex attribute descriptor object.</param>
        public VertexAttributeDescriptor(IntPtr handle) : base(handle) { }
    }
}