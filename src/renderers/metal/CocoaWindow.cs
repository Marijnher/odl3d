using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public class CocoaWindow : ObjCObject
    {
        public ContentView ContentView => Get<ContentView>("contentView");

        public CocoaWindow(IntPtr handle) : base(handle) { }

        public static CocoaWindow From(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new RenderException("GLFW did not provide an NSWindow for the Metal layer.");
            return new CocoaWindow(handle);
        }
    }
}