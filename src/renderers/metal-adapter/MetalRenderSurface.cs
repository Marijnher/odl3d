using System;

namespace odl3d.Renderer.MetalAdapter;

public class MetalRenderSurface : IRenderSurface
{
    private Metal.MetalLayer MetalLayer;

    public int Width => throw new NotImplementedException();
    public int Height => throw new NotImplementedException();
    public bool VSync
    {
        get => throw new NotImplementedException();
        set => MetalLayer.SetDisplaySyncEnabled(value);
    }
    public bool Disposed { get; private set; }
    
    public TextureFormat ColorFormat => throw new NotImplementedException();
    public TextureFormat? DepthFormat => throw new NotImplementedException();
    public int SampleCount => throw new NotImplementedException();

    public MetalRenderSurface(Metal.MetalLayer metalLayer)
    {
        MetalLayer = metalLayer;
    }

    public void Resize(int width, int height)
    {
        throw new NotImplementedException();
    }
    
    public IFrame? AcquireFrame()
    {
        return null;
    }

    public void Dispose()
    {
        Disposed = true;
    }
}