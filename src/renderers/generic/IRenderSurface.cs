using System;

namespace odl3d.Renderer;

public interface IRenderSurface : IGPUResource
{
    int Width { get; }
    int Height { get; }
    bool VSync { get; set; }
    
    TextureFormat ColorFormat { get; }
    TextureFormat? DepthFormat { get; }
    int SampleCount { get; }

    void Resize(int width, int height);
    
    IFrame? AcquireFrame();
}