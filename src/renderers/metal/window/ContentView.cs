using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public class ContentView : ObjCObject
    {
        public ContentView(IntPtr handle) : base(handle) { }

        public void SetWantsLayer(nint layer) => Send("setWantsLayer:", layer);
        
        public void SetLayer(MetalLayer metalLayer) => Send("setLayer:", metalLayer);
    }
}