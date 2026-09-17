using System;
using odl3d;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class MetalLayer : ObjCObject
    {
        private static IntPtr ClassPointer => Class("CAMetalLayer");

        public Device Device
        {
            get => Get<Device>("device");
            set => Send("setDevice:", value);
        }
        
        public nuint PixelFormat
        {
            get => GetUInt32("pixelFormat");
            set => Send("setPixelFormat:", value);
        }
        
        public bool PresentsWithTransaction
        {
            get => GetInt32("presentsWithTransaction") == 1;
            set => Send("setPresentsWithTransaction:", value ? 1 : 0);
        }

        public bool DisplaySyncEnabled
        {
            get => GetInt32("displaySyncEnabled") == 1;
            set => Send("setDisplaySyncEnabled:", value ? 1 : 0);
        }
        
        public NSRect Frame
        {
            get => GetRect("frame");
            set => Send("setFrame:", value);
        }
        
        public NSRect Bounds
        {
            get => GetRect("bounds");
            set => Send("setBounds:", value);
        }

        public double ContentsScale
        {
            get => GetDouble("contentsScale");
            set => Send("setContentsScale:", value);
        }

        public CGSize DrawableSize
        {
            get => GetSize("drawableSize");
            set => Send("setDrawableSize:", value);
        }

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
            metalLayer.Device = device;
            metalLayer.PixelFormat = GetPixelFormat(TextureFormat.BGRA8Unorm);
            metalLayer.PresentsWithTransaction = false;

            ContentView contentView = cocoaWindow.ContentView;
            contentView.SetWantsLayer(1);
            contentView.SetLayer(metalLayer);

            NSRect bounds = contentView.Bounds;
            double contentScale = contentView.BackingScaleFactor;
            double windowScale = cocoaWindow.BackingScaleFactor;
            if (contentScale != windowScale)
                Console.WriteLine($"WARNING: content scale ({contentScale}) and window scale ({windowScale}) are not equal.");

            metalLayer.Frame = bounds;
            metalLayer.Bounds = bounds;
            metalLayer.ContentsScale = contentScale;
            metalLayer.DrawableSize = new CGSize {
                Width = bounds.Size.Width * contentScale,
                Height = bounds.Size.Height * contentScale
            };
            return metalLayer;
        }

        public Drawable NextDrawable() => Get<Drawable>("nextDrawable");
    }
}
