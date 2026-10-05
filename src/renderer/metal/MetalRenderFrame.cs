using System;
using System.Collections.Generic;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a render frame in the Metal rendering pipeline.
/// </summary>
internal class MetalRenderFrame : IRenderFrame
{
    private Metal.Device Device;
    private Metal.CommandQueue CommandQueue;
    private Metal.Drawable? Drawable;
    private Metal.ObjCObject ColorTarget;
    private Metal.Texture DepthTarget;
    private Metal.Texture? OffscreenColor;
    private uint OffscreenWidth;
    private uint OffscreenHeight;
    private Metal.CommandBuffer CommandBuffer;
    private MetalRenderPass? LastPass;

    /// <summary>
    /// Gets a value indicating whether the render frame has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalRenderFrame"/> class that renders into a window drawable.
    /// </summary>
    /// <param name="device">The Metal device associated with the render frame.</param>
    /// <param name="commandQueue">The command queue used for issuing rendering commands.</param>
    /// <param name="depthTarget">The depth texture of the surface.</param>
    /// <param name="drawable">The drawable representing the render target.</param>
    public MetalRenderFrame(Metal.Device device, Metal.CommandQueue commandQueue, Metal.Texture depthTarget, Metal.Drawable drawable)
    {
        Device = device;
        CommandQueue = commandQueue;
        DepthTarget = depthTarget;
        Drawable = drawable;
        ColorTarget = drawable.Texture;
        CommandBuffer = CommandQueue.CreateCommandBuffer();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalRenderFrame"/> class that renders into an offscreen texture.
    /// </summary>
    /// <param name="device">The Metal device associated with the render frame.</param>
    /// <param name="commandQueue">The command queue used for issuing rendering commands.</param>
    /// <param name="depthTarget">The depth texture of the surface.</param>
    /// <param name="colorTarget">The BGRA8 color texture of the surface; must use managed storage.</param>
    /// <param name="width">The width of the color texture.</param>
    /// <param name="height">The height of the color texture.</param>
    public MetalRenderFrame(Metal.Device device, Metal.CommandQueue commandQueue, Metal.Texture depthTarget, Metal.Texture colorTarget, uint width, uint height)
    {
        Device = device;
        CommandQueue = commandQueue;
        DepthTarget = depthTarget;
        ColorTarget = colorTarget;
        OffscreenColor = colorTarget;
        OffscreenWidth = width;
        OffscreenHeight = height;
        CommandBuffer = CommandQueue.CreateCommandBuffer();
    }

    /// <summary>
    /// Creates a render pass for the specified render pass description.
    /// </summary>
    /// <param name="renderPassDescription">The description of the render pass to create.</param>
    /// <returns>The created render pass.</returns>
    public IRenderPass CreateRenderPass(RenderPassDescription renderPassDescription)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        LastPass = new MetalRenderPass(Device, CommandQueue, ColorTarget, DepthTarget, CommandBuffer, renderPassDescription);
        return LastPass;
    }

    /// <summary>
    /// Submits the work recorded so far, waits for it, and reads back the offscreen color target as RGBA8 with row 0 at the top.
    /// </summary>
    /// <returns>The pixels of the color target.</returns>
    public byte[] ReadPixels()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        if (OffscreenColor == null)
            throw new RenderException("ReadPixels is only supported on frames of offscreen surfaces.");
        if (LastPass is { Disposed: false })
            throw new RenderException("All render passes must be ended before reading pixels.");

        using (Metal.BlitCommandEncoder blit = CommandBuffer.CreateBlitCommandEncoder())
        {
            blit.SynchronizeResource(OffscreenColor);
            blit.EndEncoding();
        }
        CommandBuffer.Commit();
        CommandBuffer.WaitUntilCompleted();
        // A committed command buffer cannot be reused, so further passes go into a fresh one.
        CommandBuffer = CommandQueue.CreateCommandBuffer();

        byte[] pixels = new byte[OffscreenWidth * OffscreenHeight * 4];
        OffscreenColor.GetBytes(pixels, OffscreenWidth * 4, OffscreenWidth, OffscreenHeight);
        // Pipelines render to BGRA8; swap to the RGBA order promised by the API.
        for (int i = 0; i < pixels.Length; i += 4)
            (pixels[i], pixels[i + 2]) = (pixels[i + 2], pixels[i]);
        return pixels;
    }
    
    /// <summary>
    /// Presents the render frame to the display, or just submits the work of an offscreen frame.
    /// </summary>
    public void Present()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        try
        {
            if (Drawable != null) CommandBuffer.Present(Drawable);
            CommandBuffer.Commit();
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Disposes the render frame and releases all associated resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        CommandBuffer.Dispose();
        Disposed = true;
    }
}