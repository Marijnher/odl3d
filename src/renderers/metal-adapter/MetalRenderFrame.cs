using System;
using System.Collections.Generic;

namespace odl3d.Renderer.MetalAdapter;

public class MetalRenderFrame : IRenderFrame
{
    private Metal.Device Device;
    private MetalRenderSurface RenderSurface;
    private Metal.CommandQueue CommandQueue;
    private Metal.Drawable Drawable;
    private Metal.CommandBuffer CommandBuffer;

    public ITexture ColorTexture => throw new NotImplementedException();
    public bool Disposed { get; private set; }

    public MetalRenderFrame(Metal.Device device, MetalRenderSurface renderSurface, Metal.CommandQueue commandQueue, Metal.Drawable drawable)
    {
        Device = device;
        RenderSurface = renderSurface;
        CommandQueue = commandQueue;
        Drawable = drawable;
        CommandBuffer = CommandQueue.CreateCommandBuffer();
    }

    public IRenderPass CreateRenderPass(RenderPassDescription renderPassDescription) =>
        new MetalRenderPass(Device, RenderSurface, Drawable, CommandBuffer, renderPassDescription);
    
    public void Present()
    {
        CommandBuffer.Present(Drawable);
        CommandBuffer.Commit();
    }

    public void Dispose()
    {
        if (Disposed) return;
        CommandBuffer.Dispose();
        Disposed = true;
    }
}