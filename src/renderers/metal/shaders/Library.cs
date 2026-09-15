using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class Library : ObjCObject
    {
        public Library(IntPtr handle) : base(handle, true) { }

        public ObjCObject NewFunctionWithName(string name)
        {
            IntPtr handle = Send("newFunctionWithName:", name);
            if (handle == IntPtr.Zero)
                throw new RenderException($"Metal could not find shader function '{name}'.");
            return new ObjCObject(handle, true);
        }
    }
}