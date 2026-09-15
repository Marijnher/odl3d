using System;
using odl3d;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class MetalLayer : ObjCObject
    {
        private static IntPtr ClassPointer => Class("CAMetalLayer");

        private MetalLayer(IntPtr handle) : base(handle) { }

        public static MetalLayer Create()
        {
            IntPtr handle = SendRaw(ClassPointer, "layer");
            if (handle == IntPtr.Zero)
                throw new RenderException("Could not create CAMetalLayer.");
            return new MetalLayer(handle);
        }

        public static MetalLayer AttachToWindow(Device device, IntPtr glfwWindow)
        {
            IntPtr handle = GLFW.glfwGetCocoaWindow(glfwWindow);
            if (handle == IntPtr.Zero)
                throw new RenderException("GLFW did not provide an NSWindow for the Metal layer.");
            CocoaWindow cocoaWindow = new CocoaWindow(handle);
            
            MetalLayer metalLayer = Create();
            metalLayer.SetDevice(device);
            metalLayer.SetPixelFormat(80); // MTLPixelFormatBGRA8Unorm
            metalLayer.SetPresentsWithTransaction(0);

            ContentView contentView = cocoaWindow.ContentView;
            contentView.SetWantsLayer(1);
            contentView.SetLayer(metalLayer);
            return metalLayer;
        }

        public Drawable NextDrawable() => Get<Drawable>("nextDrawable");

        public void SetDevice(Device device) => Send("setDevice:", device);

        public void SetPixelFormat(nuint format) => Send("setPixelFormat:", format);

        public void SetPresentsWithTransaction(nuint value) => Send("setPresentsWithTransaction:", value);
    }
}
