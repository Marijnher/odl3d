using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class NSException : ObjCObject
    {
        public string LocalizedDescription => GetString("localizedDescription");

        public NSException(IntPtr handle) : base(handle) { }
    }
}