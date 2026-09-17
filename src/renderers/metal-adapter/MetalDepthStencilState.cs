using System;

namespace odl3d.Renderer.MetalAdapter;

public class MetalDepthStencilState : IDepthStencilState
{
    public bool DepthTestEnabled { get; }
    public bool Disposed { get; private set; }

    public Metal.DepthStencilState DepthStencilState;

    public MetalDepthStencilState(Metal.Device device, DepthStencilDescription description)
    {
        DepthTestEnabled = description.DepthTestEnabled;
        DepthStencilState = device.CreateDepthStencilState(
            description.DepthCompareFunction,
            description.DepthWriteEnabled
        );
    }

    public void Dispose() 
    {
        if (Disposed) return;
        DepthStencilState.Dispose();
        Disposed = true;
    }
}