using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal buffer resource that encapsulates its length and provides functionality for updating its contents.
    /// </summary>
    public sealed class Buffer : ObjCObject
    {
        /// <summary>
        /// Gets the length of the Metal buffer in bytes.
        /// </summary>
        public uint Length => GetUInt32("length");

        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer"/> class with the specified Metal buffer handle.
        /// </summary>
        /// <param name="handle">The handle to the Metal buffer.</param>
        public Buffer(IntPtr handle) : base(handle, true) { }

        /// <summary>
        /// Updates a portion of the Metal buffer with the specified generic data.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the data array.</typeparam>
        /// <param name="data">The generic data to update the buffer with.</param>
        /// <param name="offset">The offset in the buffer to start the update.</param>
        /// <param name="count">The number of elements to update.</param>
        /// <exception cref="RenderException">Thrown when the update does not fit in the Metal buffer or when the Metal buffer does not provide writable contents.</exception>
        public unsafe void Update<T>(T[] data, int offset, int count) where T : unmanaged
        {
            if ((nuint)((offset + count) * sizeof(T)) > Length)
                throw new RenderException("The update does not fit in the Metal buffer.");

            IntPtr contents = GetRaw("contents");
            if (contents == IntPtr.Zero)
                throw new RenderException("Metal did not provide writable buffer contents.");
            // Copy directly into raw buffer
            fixed (T* source = data)
            {
                System.Buffer.MemoryCopy(source + offset, (void*) contents, count * sizeof(T), count * sizeof(T));
            }
        }
    }
}
