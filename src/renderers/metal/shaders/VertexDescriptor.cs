using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal vertex descriptor, which describes the layout of vertex attributes in a vertex buffer.
    /// </summary>
    public sealed class VertexDescriptor : ObjCObject
    {
        /// <summary>
        /// Gets the class pointer for the Metal vertex descriptor.
        /// </summary>
        private static IntPtr ClassPointer => Class("MTLVertexDescriptor");

        /// <summary>
        /// Gets the array of vertex attribute descriptors for this vertex descriptor.
        /// </summary>
        public VertexAttributeDescriptorArray Attributes =>
            Get<VertexAttributeDescriptorArray>("attributes");

        /// <summary>
        /// Gets the array of vertex layout descriptors for this vertex descriptor.
        /// </summary>
        public VertexLayoutDescriptorArray Layouts =>
            Get<VertexLayoutDescriptorArray>("layouts");

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexDescriptor"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Metal vertex descriptor object.</param>
        public VertexDescriptor(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Creates a new instance of the <see cref="VertexDescriptor"/> class.
        /// </summary>
        /// <returns>A new <see cref="VertexDescriptor"/> instance.</returns>
        public static VertexDescriptor Create() =>
            new VertexDescriptor(SendRaw(ClassPointer, "new"));
    }
}