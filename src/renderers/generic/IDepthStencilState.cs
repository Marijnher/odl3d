using System;

namespace odl3d.Renderer;

public interface IDepthStencilState : IGPUResource
{
    public bool DepthTestEnabled { get; }
}