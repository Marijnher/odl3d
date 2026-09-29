using System;
using System.Collections.Generic;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a render frame in the Metal rendering pipeline.
/// </summary>
internal class MetalRenderFrame : IRenderFrame
{
    private Metal.Device Device;
    private MetalRenderSurface RenderSurface;
    private Metal.CommandQueue CommandQueue;
    private Metal.Drawable Drawable;
    private Metal.CommandBuffer CommandBuffer;

    /// <summary>
    /// Gets the color texture associated with the render frame.
    /// </summary>
    public ITexture ColorTexture => throw new NotImplementedException();

    /// <summary>
    /// Gets a value indicating whether the render frame has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalRenderFrame"/> class.
    /// </summary>
    /// <param name="device">The Metal device associated with the render frame.</param>
    /// <param name="renderSurface">The render surface associated with the render frame.</param>
    /// <param name="commandQueue">The command queue used for issuing rendering commands.</param>
    /// <param name="drawable">The drawable representing the render target.</param>
    public MetalRenderFrame(Metal.Device device, MetalRenderSurface renderSurface, Metal.CommandQueue commandQueue, Metal.Drawable drawable)
    {
        Device = device;
        RenderSurface = renderSurface;
        CommandQueue = commandQueue;
        Drawable = drawable;
        CommandBuffer = CommandQueue.CreateCommandBuffer();
    }

    /// <summary>
    /// Creates a render pass for the specified render pass description.
    /// </summary>
    /// <param name="renderPassDescription">The description of the render pass to create.</param>
    /// <returns>The created render pass.</returns>
    public IRenderPass CreateRenderPass(RenderPassDescription renderPassDescription) =>
        new MetalRenderPass(Device, RenderSurface, CommandQueue, Drawable, CommandBuffer, renderPassDescription);
    
    /// <summary>
    /// Presents the render frame to the display.
    /// </summary>
    public void Present()
    {
        CommandBuffer.Present(Drawable);
        CommandBuffer.Commit();
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