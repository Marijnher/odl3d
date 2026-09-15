using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class CocoaWindow : ObjCObject
    {
        public ContentView ContentView => Get<ContentView>("contentView");

        public CocoaWindow(IntPtr handle) : base(handle) { }
    }
}