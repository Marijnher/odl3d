using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class NSString : ObjCObject
    {
        private static IntPtr ClassPointer => obcj_getClass("NSString");

        public string Value =>
            Marshal.PtrToStringUTF8(GetRaw("UTF8String")) ??
                throw new RenderException("Failed to convert NSString to C# String");

        public NSString(IntPtr handle) : base(handle) { }

        public static NSString Create(string value)
        {
            // stringWithUTF8String: returns an autoreleased NSString; only the
            // temporary UTF-8 allocation is owned here.
            IntPtr utf8 = Marshal.StringToCoTaskMemUTF8(value);
            try
            {
                return new NSString(SendRaw(ClassPointer, "stringWithUTF8String:", utf8));
            }
            finally
            {
                Marshal.FreeCoTaskMem(utf8);
            }
        }
    }
}