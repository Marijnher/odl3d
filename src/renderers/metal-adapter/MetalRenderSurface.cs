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
        get => MetalLayer.DisplaySyncEnabled;
        set => MetalLayer.DisplaySyncEnabled = value;
    }
    public bool Disposed { get; private set; }
    
    public TextureFormat ColorFormat => throw new NotImplementedException();
    public TextureFormat? DepthFormat => throw new NotImplementedException();
    public int SampleCount => throw new NotImplementedException();

    public Metal.Texture DepthTexture { get; private set; }

    public MetalRenderSurface(Metal.Device device, nint windowHandle)
    {
        Device = device;
        MetalLayer = Metal.MetalLayer.AttachToWindow(Device, windowHandle);
        CommandQueue = Device.NewCommandQueue();
        DepthTexture = Device.CreateTexture(Width, Height, TextureFormat.Depth32Float);
    }

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