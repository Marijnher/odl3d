using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a native Objective-C NSString object, which encapsulates a string value.
    /// </summary>
    public sealed class NSString : ObjCObject
    {
        /// <summary>
        /// Gets the native class pointer for the NSString class.
        /// </summary>
        private static IntPtr ClassPointer => obcj_getClass("NSString");

        /// <summary>
        /// Gets the string value of the NSString object.
        /// </summary>
        public string Value =>
            Marshal.PtrToStringUTF8(GetRaw("UTF8String")) ??
                throw new RenderException("Failed to convert NSString to C# String");

        /// <summary>
        /// Initializes a new instance of the NSString class with the specified native handle.
        /// </summary>
        /// <param name="handle">The native handle of the NSString object.</param>
        public NSString(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Creates a new NSString object from the specified C# string value.
        /// </summary>
        /// <param name="value">The C# string value to convert to an NSString.</param>
        /// <returns>A new NSString instance representing the specified string value.</returns>
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