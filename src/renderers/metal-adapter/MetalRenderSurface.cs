using System;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a Metal render surface, encapsulating the Metal layer, command queue, and associated resources.
/// </summary>
internal class MetalRenderSurface : IRenderSurface
{
    private Metal.Device Device;
    private Metal.MetalLayer MetalLayer;
    private Metal.CommandQueue CommandQueue;

    /// <summary>
    /// Gets the width of the render surface in pixels.
    /// </summary>
    public uint Width => (uint) MetalLayer.DrawableSize.Width;

    /// <summary>
    /// Gets the height of the render surface in pixels.
    /// </summary>
    public uint Height => (uint) MetalLayer.DrawableSize.Height;

    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization (VSync) is enabled for the render surface.
    /// </summary>
    public bool VSync
    {
        get => MetalLayer.DisplaySyncEnabled;
        set => MetalLayer.DisplaySyncEnabled = value;
    }

    /// <summary>
    /// Gets a value indicating whether the render surface has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }
    
    /// <summary>
    /// Gets the color format of the render surface.
    /// </summary>
    public TextureFormat ColorFormat => throw new NotImplementedException();

    /// <summary>
    /// Gets the depth format of the render surface, if any.
    /// </summary>
    public TextureFormat? DepthFormat => throw new NotImplementedException();

    /// <summary>
    /// Gets the number of samples used for multisampling, if any.
    /// </summary>
    public int SampleCount => throw new NotImplementedException();

    /// <summary>
    /// Gets the depth texture associated with the render surface.
    /// </summary>
    public Metal.Texture DepthTexture { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalRenderSurface"/> class with the specified Metal device and window handle.
    /// </summary>
    /// <param name="device">The Metal device used to create the render surface.</param>
    /// <param name="windowHandle">The handle to the window to which the render surface is attached.</param>
    public MetalRenderSurface(Metal.Device device, nint windowHandle)
    {
        Device = device;
        MetalLayer = Metal.MetalLayer.AttachToWindow(Device, windowHandle);
        CommandQueue = Device.NewCommandQueue();
        DepthTexture = Device.CreateTexture(Width, Height, TextureFormat.Depth32Float);
    }

    /// <summary>
    /// Resizes the render surface to the specified width and height.
    /// </summary>
    /// <param name="width">The new width of the render surface.</param>
    /// <param name="height">The new height of the render surface.</param>
    public void Resize(int width, int height)
    {
        DepthTexture.Dispose();
        DepthTexture = Device.CreateTexture(Width, Height, TextureFormat.Depth32Float);
        MetalLayer.DrawableSize = new Metal.CGSize() {
            Width = width * MetalLayer.ContentsScale,
            Height = height * MetalLayer.ContentsScale
        };
        MetalLayer.Bounds = new Metal.NSRect
        {
            Origin = new Metal.NSPoint() { X = 0, Y = 0 },
            Size = new Metal.NSSize { Width = width, Height = height }
        };
        MetalLayer.Frame = MetalLayer.Bounds;
    }
    
    /// <summary>
    /// Acquires the next available render frame from the render surface.
    /// </summary>
    /// <returns>The acquired render frame, or null if no frame is available.</returns>
    public IRenderFrame? AcquireFrame()
    {
        var drawable = MetalLayer.NextDrawable();
        return new MetalRenderFrame(Device, this, CommandQueue, drawable);
    }

    /// <summary>
    /// Disposes of the render surface and releases all associated resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        CommandQueue.Dispose();
        DepthTexture.Dispose();
        MetalLayer.Dispose();
        Disposed = true;
    }
}