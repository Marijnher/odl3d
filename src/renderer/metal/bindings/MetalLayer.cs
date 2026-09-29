using System;
using odl3d;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal layer that can be attached to a window for rendering with Metal.
    /// </summary>
    public sealed class MetalLayer : ObjCObject
    {
        /// <summary>
        /// Gets the Objective-C class pointer for the CAMetalLayer class.
        /// </summary>
        private static IntPtr ClassPointer => Class("CAMetalLayer");

        /// <summary>
        /// Gets or sets the Metal device associated with the layer.
        /// </summary>
        public Device Device
        {
            get => Get<Device>("device");
            set => Send("setDevice:", value);
        }
        
        /// <summary>
        /// Gets or sets the pixel format of the Metal layer.
        /// </summary>
        public nuint PixelFormat
        {
            get => GetUInt32("pixelFormat");
            set => Send("setPixelFormat:", value);
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the Metal layer presents its content with a transaction.
        /// </summary>
        public bool PresentsWithTransaction
        {
            get => GetInt32("presentsWithTransaction") == 1;
            set => Send("setPresentsWithTransaction:", value ? 1 : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Metal layer's display sync is enabled.
        /// </summary>
        public bool DisplaySyncEnabled
        {
            get => GetInt32("displaySyncEnabled") == 1;
            set => Send("setDisplaySyncEnabled:", value ? 1 : 0);
        }
        
        /// <summary>
        /// Gets or sets the frame rectangle of the Metal layer.
        /// </summary>
        public NSRect Frame
        {
            get => GetRect("frame");
            set => Send("setFrame:", value);
        }
        
        /// <summary>
        /// Gets or sets the bounds rectangle of the Metal layer.
        /// </summary>
        public NSRect Bounds
        {
            get => GetRect("bounds");
            set => Send("setBounds:", value);
        }

        /// <summary>
        /// Gets or sets the contents scale factor of the Metal layer.
        /// </summary>
        public double ContentsScale
        {
            get => GetDouble("contentsScale");
            set => Send("setContentsScale:", value);
        }

        /// <summary>
        /// Gets or sets the drawable size of the Metal layer.
        /// </summary>
        public CGSize DrawableSize
        {
            get => GetSize("drawableSize");
            set => Send("setDrawableSize:", value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetalLayer"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying CAMetalLayer object.</param>
        private MetalLayer(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Creates a new instance of the <see cref="MetalLayer"/> class.
        /// </summary>
        /// <returns>A new <see cref="MetalLayer"/> instance.</returns>
        /// <exception cref="RenderException">Thrown if the Metal layer could not be created.</exception>
        public static MetalLayer Create()
        {
            IntPtr handle = SendRaw(ClassPointer, "layer");
            if (handle == IntPtr.Zero)
                throw new RenderException("Could not create CAMetalLayer.");
            return new MetalLayer(handle);
        }

        /// <summary>
        /// Attaches a new Metal layer to the specified GLFW window using the given Metal device.
        /// </summary>
        /// <param name="device">The Metal device to use for the layer.</param>
        /// <param name="glfwWindow">The handle to the GLFW window to attach the Metal layer to.</param>
        /// <returns>A new <see cref="MetalLayer"/> instance attached to the specified window.</returns>
        /// <exception cref="RenderException">Thrown if the Metal layer could not be attached to the window.</exception>
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
            contentView.WantsLayer = true;
            contentView.Layer = metalLayer;

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

        /// <summary>
        /// Gets the next drawable from the Metal layer.
        /// </summary>
        /// <returns>The next <see cref="Drawable"/> from the Metal layer.</returns>
        public Drawable NextDrawable() => Get<Drawable>("nextDrawable");
    }
}
