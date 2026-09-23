using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a drawable object in Metal, which typically corresponds to a renderable texture.
    /// </summary>
    public sealed class Drawable : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the Drawable class with the specified native handle.
        /// </summary>
        /// <param name="handle">The native handle for the drawable object.</param>
        public Drawable(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Gets the texture associated with this drawable.
        /// </summary>
        public ObjCObject Texture => Get("texture");
    }
}
