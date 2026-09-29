using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a descriptor for configuring a depth-stencil state in Metal.
    /// </summary>
    public sealed class DepthStencilDescriptor : ObjCObject
    {
        /// <summary>
        /// Gets the native class pointer for the MTLDepthStencilDescriptor Objective-C class.
        /// </summary>
        public static IntPtr ClassPointer => Class("MTLDepthStencilDescriptor");

        /// <summary>
        /// Gets or sets the depth compare function for the depth-stencil state.
        /// </summary>
        public CompareFunction DepthCompareFunction
        {
            get => (CompareFunction) GetUInt32("depthCompareFunction");
            set => Send("setDepthCompareFunction:", (nuint) value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether depth writing is enabled for the depth-stencil state.
        /// </summary>
        public bool DepthWriteEnabled
        {
            get => GetInt32("depthWriteEnabled") != 0;
            set => Send("setDepthWriteEnabled:", value ? 1 : 0);
        }

        /// <summary>
        /// Initializes a new instance of the DepthStencilDescriptor class with the specified native handle.
        /// </summary>
        /// <param name="handle"></param>
        public DepthStencilDescriptor(IntPtr handle) : base(handle, true) { }

        /// <summary>
        /// Creates a new instance of the DepthStencilDescriptor class.
        /// </summary>
        /// <returns>A new instance of the DepthStencilDescriptor class.</returns>
        public static DepthStencilDescriptor Create() =>
            new DepthStencilDescriptor(SendRaw(ClassPointer, "new"));
    }

    /// <summary>
    /// Represents a depth-stencil state in Metal.
    /// </summary>
    public sealed class DepthStencilState : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the DepthStencilState class with the specified native handle.
        /// </summary>
        /// <param name="handle">The native handle for the depth-stencil state.</param>
        public DepthStencilState(IntPtr handle) : base(handle, true) { }
    }
}
