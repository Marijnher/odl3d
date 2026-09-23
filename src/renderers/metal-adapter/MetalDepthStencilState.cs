using System;

namespace odl3d.Renderer.MetalAdapter;

internal class MetalDepthStencilState : IDepthStencilState
{
    public bool DepthTestEnabled { get; }
    public bool DepthWriteEnabled { get; }
    public CompareFunction DepthCompareFunction { get; }

    public bool Disposed { get; private set; }

    public Metal.DepthStencilState DepthStencilState;

    public MetalDepthStencilState(Metal.Device device, DepthStencilDescription description)
    {
        DepthTestEnabled = description.DepthTestEnabled;
        DepthWriteEnabled = description.DepthWriteEnabled;
        DepthCompareFunction = description.DepthCompareFunction;
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