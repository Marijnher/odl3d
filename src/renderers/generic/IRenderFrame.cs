using System;
using System.Collections.Generic;
using odl3d;

namespace odl3d.Renderer;

public interface IRenderFrame : IDisposable
{
    ITexture ColorTexture { get; }

    IRenderPass CreateRenderPass(RenderPassDescription renderPassDescription);
    
    void Present();
}