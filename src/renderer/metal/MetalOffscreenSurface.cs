using System;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a Metal render surface that renders into textures instead of a window.
/// </summary>
internal class MetalOffscreenSurface : IRenderSurface
{
    private readonly Metal.Device Device;
    private readonly Metal.CommandQueue CommandQueue;
    private Metal.Texture ColorTexture;
    private Metal.Texture DepthTexture;

    /// <summary>
    /// Gets the width of the render surface in pixels.
    /// </summary>
    public uint Width { get; private set; }

    /// <summary>
    /// Gets the height of the render surface in pixels.
    /// </summary>
    public uint Height { get; private set; }

    /// <summary>
    /// Gets or sets the VSync flag. Offscreen surfaces never wait for the display, so this has no effect.
    /// </summary>
    public bool VSync { get; set; }

    /// <summary>
    /// Gets the depth format of the render surface.
    /// </summary>
    public TextureFormat? DepthFormat => TextureFormat.Depth32Float;

    /// <summary>
    /// Gets the sample count of the render surface.
    /// </summary>
    public int SampleCount => 1;

    /// <summary>
    /// Gets a value indicating whether the render surface has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalOffscreenSurface"/> class.
    /// </summary>
    /// <param name="device">The Metal device used to create the render targets.</param>
    /// <param name="width">The width of the surface in pixels.</param>
    /// <param name="height">The height of the surface in pixels.</param>
    public MetalOffscreenSurface(Metal.Device device, uint width, uint height)
    {
        if (width == 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height == 0) throw new ArgumentOutOfRangeException(nameof(height));
        Device = device;
        CommandQueue = Device.NewCommandQueue();
        (ColorTexture, DepthTexture) = CreateTargets(width, height);
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Resizes the render surface, discarding its current contents.
    /// </summary>
    /// <param name="width">The new width of the render surface in pixels.</param>
    /// <param name="height">The new height of the render surface in pixels.</param>
    public void Resize(int width, int height)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        ColorTexture.Dispose();
        DepthTexture.Dispose();
        (ColorTexture, DepthTexture) = CreateTargets((uint) width, (uint) height);
        Width = (uint) width;
        Height = (uint) height;
    }

    /// <summary>
    /// Acquires a new render frame that renders into the offscreen textures.
    /// </summary>
    /// <returns>The acquired render frame.</returns>
    public IRenderFrame? AcquireFrame()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        return new MetalRenderFrame(Device, CommandQueue, DepthTexture, ColorTexture, Width, Height);
    }

    // Color is BGRA8 because Metal pipelines are created for that attachment format.
    private (Metal.Texture Color, Metal.Texture Depth) CreateTargets(uint width, uint height) => (
        Device.CreateRenderTargetTexture(width, height, TextureFormat.BGRA8Unorm),
        Device.CreateRenderTargetTexture(width, height, TextureFormat.Depth32Float));

    /// <summary>
    /// Disposes the render surface and its render targets.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        ColorTexture.Dispose();
        DepthTexture.Dispose();
        CommandQueue.Dispose();
        Disposed = true;
    }
}
