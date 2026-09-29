using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Cocoa window in the Metal renderer, providing access to its content view and backing scale factor.
    /// </summary>
    public sealed class CocoaWindow : ObjCObject
    {
        /// <summary>
        /// Gets the content view of the Cocoa window.
        /// </summary>
        public ContentView ContentView => Get<ContentView>("contentView");

        /// <summary>
        /// Gets the backing scale factor of the Cocoa window.
        /// </summary>
        public double BackingScaleFactor => GetDouble("backingScaleFactor");

        /// <summary>
        /// Initializes a new instance of the <see cref="CocoaWindow"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Cocoa window object.</param>
        public CocoaWindow(IntPtr handle) : base(handle) { }
    }
}