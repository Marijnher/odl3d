using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a native Objective-C NSException object, which encapsulates information about an exception that occurred in the application.
    /// </summary>
    public sealed class NSException : ObjCObject
    {
        /// <summary>
        /// Gets the localized description of the exception.
        /// </summary>
        public string LocalizedDescription => GetString("localizedDescription");

        /// <summary>
        /// Initializes a new instance of the NSException class with the specified native handle.
        /// </summary>
        /// <param name="handle">The native handle of the NSException object.</param>
        public NSException(IntPtr handle) : base(handle) { }
    }
}