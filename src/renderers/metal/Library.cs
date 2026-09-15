using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderers;

public static partial class Metal
{
    public class Library : ObjCObject
    {
        public Library(IntPtr handle) : base(handle) { }

        public ObjCObject NewFunctionWithName(string name)
        {
            IntPtr handle = Send("newFunctionWithName:", name);
            if (handle == IntPtr.Zero)
                throw new RenderException($"Metal could not find shader function '{name}'.");
            return new ObjCObject(handle);
        }
    }
}