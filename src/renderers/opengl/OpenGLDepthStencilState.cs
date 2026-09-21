using System;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLDepthStencilState : IDepthStencilState
{
    public bool DepthTestEnabled { get; }
    public bool DepthWriteEnabled { get; }
    public CompareFunction DepthCompareFunction { get; }

    public bool Disposed { get; private set; }

    public OpenGLDepthStencilState(DepthStencilDescription description)
    {
        DepthTestEnabled = description.DepthTestEnabled;
        DepthWriteEnabled = description.DepthWriteEnabled;
        DepthCompareFunction = description.DepthCompareFunction;
    }

    public void Dispose() 
    {
        if (Disposed) return;
        Disposed = true;
    }
}