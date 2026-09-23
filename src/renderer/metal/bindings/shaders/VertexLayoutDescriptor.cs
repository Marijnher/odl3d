using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents an array of Metal vertex layout descriptors.
    /// </summary>
    public sealed class VertexLayoutDescriptorArray : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VertexLayoutDescriptorArray"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Metal vertex layout descriptor array object.</param>
        public VertexLayoutDescriptorArray(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Gets the vertex layout descriptor at the specified index.
        /// </summary>
        /// <param name="index">The index of the vertex layout descriptor to retrieve.</param>
        /// <returns>The <see cref="VertexLayoutDescriptor"/> at the specified index.</returns>
        public VertexLayoutDescriptor Get(int index) =>
            Send<VertexLayoutDescriptor>("objectAtIndexedSubscript:", index);

        /// <summary>
        /// Gets the vertex layout descriptor at the specified index.
        /// </summary>
        /// <param name="index">The index of the vertex layout descriptor to retrieve.</param>
        /// <returns>The <see cref="VertexLayoutDescriptor"/> at the specified index.</returns>
        public VertexLayoutDescriptor this[int index] => Get(index);
    }

    /// <summary>
    /// Represents a Metal vertex layout descriptor, which describes the layout of vertex data in a vertex buffer.
    /// </summary>
    public sealed class VertexLayoutDescriptor : ObjCObject
    {
        /// <summary>
        /// Gets or sets the stride, in bytes, of the vertex data in the vertex buffer.
        /// </summary>
        public uint Stride
        {
            get => GetUInt32("stride");
            set => Send("setStride:", value);
        }

        /// <summary>
        /// Gets or sets the step function, which determines how often the vertex data is updated.
        /// </summary>
        public StepMode StepFunction
        {
            get => GetUInt32("stepFunction") switch
            {
                1 => StepMode.PerVertex,
                2 => StepMode.PerInstance,
                uint other => throw new RenderException($"Unsupported step function: {other}")
            };
            set => Send("setStepFunction:", value switch
            {
                StepMode.PerVertex => 1,
                StepMode.PerInstance => 2,
                _ => throw new RenderException($"Unsupported step function: {value}")
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexLayoutDescriptor"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Metal vertex layout descriptor object.</param>
        public VertexLayoutDescriptor(IntPtr handle) : base(handle) { }
    }
}