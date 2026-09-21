using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class Buffer : ObjCObject
    {
        public uint Length => GetUInt32("length");

        public Buffer(IntPtr handle) : base(handle, true) { }

        public void Update(float[] data, int offset, int count)
        {
            if ((nuint)((offset + count) * sizeof(float)) > Length)
                throw new ArgumentException("The update does not fit in the Metal buffer.", nameof(data));

            IntPtr contents = GetRaw("contents");
            if (contents == IntPtr.Zero)
                throw new RenderException("Metal did not provide writable buffer contents.");
            // Copy directly into raw buffer
            Marshal.Copy(data, offset, contents, count);
        }

        public unsafe void Update<T>(T[] data, int offset, int count) where T : unmanaged
        {
            if ((nuint)((offset + count) * sizeof(T)) > Length)
                throw new ArgumentException("The update does not fit in the Metal buffer.", nameof(data));

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
