using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class CocoaWindow : ObjCObject
    {
        public ContentView ContentView => Get<ContentView>("contentView");
        public double BackingScaleFactor => GetDouble("backingScaleFactor");

        public CocoaWindow(IntPtr handle) : base(handle) { }
    }
}