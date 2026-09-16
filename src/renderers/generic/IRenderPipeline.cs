using System;

namespace odl3d.Renderer;

public interface IRenderPipeline : IGPUResource
{
    public PrimitiveType PrimitiveType { get; }
}