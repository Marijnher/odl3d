using System;
using odl3d;

namespace odl3d.Renderers;

public static partial class MetalNew
{
    /// <summary>Owns a CAMetalLayer and supplies the drawable presented by a frame.</summary>
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
            CocoaWindow cocoaWindow = CocoaWindow.From(GLFW.glfwGetCocoaWindow(glfwWindow));
            
            MetalLayer metalLayer = Create();
            metalLayer.SetDevice(device);
            metalLayer.SetPixelFormat(80); // MTLPixelFormatBGRA8Unorm
            metalLayer.SetPresentsWithTransaction(0);

            ContentView contentView = cocoaWindow.ContentView;
            contentView.SetWantsLayer(1);
            contentView.SetLayer(metalLayer);
            return metalLayer;
        }

        public Drawable NextDrawable() => new Drawable(GetRaw("nextDrawable"));

        public void SetDevice(Device device) => Send("setDevice:", device);

        public void SetPixelFormat(nuint format) => Send("setPixelFormat:", format);

        public void SetPresentsWithTransaction(nuint value) => Send("setPresentsWithTransaction:", value);
    }
}
