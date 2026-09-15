using System;

namespace odl3d.Renderer;

public sealed record DepthStencilDescription
{
    public bool DepthTestEnabled { get; init; }
    public bool DepthWriteEnabled { get; init; }

    public CompareFunction DepthCompare { get; init; } = CompareFunction.LessEqual;

    public bool StencilEnabled { get; init; }
}