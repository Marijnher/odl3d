using System;

namespace odl3d.Renderer.MetalAdapter;

public class MetalRenderSurface : IRenderSurface
{
    private Metal.Device Device;
    private Metal.MetalLayer MetalLayer;
    private Metal.CommandQueue CommandQueue;

    public uint Width => (uint) MetalLayer.DrawableSize.Width;
    public uint Height => (uint) MetalLayer.DrawableSize.Height;
    public bool VSync
    {
        get => throw new NotImplementedException();
        set => MetalLayer.SetDisplaySyncEnabled(value);
    }
    public bool Disposed { get; private set; }
    
    public TextureFormat ColorFormat => throw new NotImplementedException();
    public TextureFormat? DepthFormat => throw new NotImplementedException();
    public int SampleCount => throw new NotImplementedException();

    public Metal.Texture DepthTexture { get; }

    public MetalRenderSurface(Metal.Device device, nint windowHandle)
    {
        Device = device;
        MetalLayer = Metal.MetalLayer.AttachToWindow(Device, windowHandle);
        CommandQueue = Device.NewCommandQueue();
        DepthTexture = Device.CreateDepthTexture(Width, Height);
    }

    public void Resize(int width, int height)
    {
        throw new NotImplementedException();
    }
    
    public IRenderFrame? AcquireFrame()
    {
        var drawable = MetalLayer.NextDrawable();
        return new MetalRenderFrame(Device, this, CommandQueue, drawable);
    }

    public void Dispose()
    {
        if (Disposed) return;
        CommandQueue.Dispose();
        DepthTexture.Dispose();
        MetalLayer.Dispose();
        Disposed = true;
    }
}