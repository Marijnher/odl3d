using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class ContentView : ObjCObject
    {
        public NSRect Bounds => GetRect("bounds");
        public double BackingScaleFactor => GetDouble("backingScaleFactor");

        public ContentView(IntPtr handle) : base(handle) { }

        public void SetWantsLayer(nint layer) => Send("setWantsLayer:", layer);
        
        public void SetLayer(MetalLayer metalLayer) => Send("setLayer:", metalLayer);
    }
}