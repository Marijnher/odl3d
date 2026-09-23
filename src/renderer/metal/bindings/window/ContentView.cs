using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents the content view of a Cocoa window in the Metal renderer, providing access to its bounds and backing scale factor.
    /// </summary>
    public sealed class ContentView : ObjCObject
    {
        /// <summary>
        /// Gets the bounds of the content view.
        /// </summary>
        public NSRect Bounds => GetRect("bounds");

        /// <summary>
        /// Gets the backing scale factor of the content view.
        /// </summary>
        public double BackingScaleFactor => GetDouble("backingScaleFactor");

        /// <summary>
        /// Gets or sets a value indicating whether the content view wants a layer.
        /// </summary>
        public bool WantsLayer
        {
            get => GetInt32("wantsLayer") == 1;
            set => Send("setWantsLayer:", value ? 1 : 0);
        }

        /// <summary>
        /// Gets or sets the Metal layer of the content view.
        /// </summary>
        public MetalLayer Layer
        {
            get => Get<MetalLayer>("layer");
            set => Send("setLayer:", value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentView"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native content view object.</param>
        public ContentView(IntPtr handle) : base(handle) { }
    }
}