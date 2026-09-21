using System;

namespace odl3d.Renderer;

public sealed record DepthStencilDescription
{
    public bool DepthTestEnabled { get; init; }
    public bool DepthWriteEnabled { get; init; }
    public CompareFunction DepthCompareFunction { get; init; } = CompareFunction.Less;
}