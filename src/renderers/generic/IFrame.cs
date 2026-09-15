using System;
using odl3d;

namespace odl3d.Renderer;

public interface IFrame : IDisposable
{
    ITexture ColorTexture { get; }

    IRenderPass CreateRenderPass(RenderPassDescription renderPassDescription);
    
    void Present();
}