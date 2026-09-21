using System;

namespace odl3d.Renderer;

public interface IDepthStencilState : IGPUResource
{
    public bool DepthTestEnabled { get; }
    public bool DepthWriteEnabled { get; }
    public CompareFunction DepthCompareFunction { get; }
}