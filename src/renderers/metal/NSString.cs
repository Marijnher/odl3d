using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderers;

public static partial class MetalNew
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
            // TODO: Investigate if this is a memory leak
            IntPtr utf8 = Marshal.StringToCoTaskMemUTF8(value);
            try
            {
                return new NSString(Send(ClassPointer, "stringWithUTF8String:", utf8));
            }
            finally
            {
                Marshal.FreeCoTaskMem(utf8);
            }
        }
    }
}