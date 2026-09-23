using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal shader library, which contains compiled shader functions.
    /// </summary>
    public sealed class Library : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Library"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying Objective-C object.</param>
        public Library(IntPtr handle) : base(handle, true) { }

        /// <summary>
        /// Creates a new shader function with the specified name.
        /// </summary>
        /// <param name="name">The name of the shader function to create.</param>
        /// <returns>A new instance of the <see cref="ObjCObject"/> class representing the shader function.</returns>
        /// <exception cref="RenderException">Thrown if the shader function with the specified name could not be found.</exception>
        public ObjCObject NewFunctionWithName(string name)
        {
            IntPtr handle = Send("newFunctionWithName:", name);
            if (handle == IntPtr.Zero)
                throw new RenderException($"Metal could not find shader function '{name}'.");
            return new ObjCObject(handle, true);
        }
    }
}