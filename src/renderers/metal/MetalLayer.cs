using System;
using odl3d;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class MetalLayer : ObjCObject
    {
        private static IntPtr ClassPointer => Class("CAMetalLayer");

        public CGSize DrawableSize => GetSize("drawableSize");

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

            NSRect bounds = contentView.Bounds;
            double contentScale = contentView.BackingScaleFactor;
            double windowScale = cocoaWindow.BackingScaleFactor;
            if (contentScale != windowScale)
                Console.WriteLine($"WARNING: content scale ({contentScale}) and window scale ({windowScale}) are not equal.");

            metalLayer.SetFrame(bounds);
            metalLayer.SetBounds(bounds);
            metalLayer.SetContentsScale(contentScale);
            metalLayer.SetDrawableSize(
                new CGSize { Width = bounds.Size.Width * contentScale, Height = bounds.Size.Height * contentScale }
            );

            CGSize cgSize = metalLayer.DrawableSize;

            return metalLayer;
        }

        public Drawable NextDrawable() => Get<Drawable>("nextDrawable");

        public void SetDevice(Device device) => Send("setDevice:", device);

        public void SetPixelFormat(nuint format) => Send("setPixelFormat:", format);

        public void SetPresentsWithTransaction(nuint value) => Send("setPresentsWithTransaction:", value);

        public void SetDisplaySyncEnabled(bool enabled) =>
            Send("setDisplaySyncEnabled:", enabled ? 1 : 0);

        public void SetFrame(NSRect rect) =>
            Send("setFrame:", rect);

        public void SetBounds(NSRect rect) =>
            Send("setBounds:", rect);

        public void SetContentsScale(double scalingFactor) =>
            Send("setContentsScale:", scalingFactor);

        public void SetDrawableSize(CGSize size) =>
            Send("setDrawableSize:", size);
    }
}
