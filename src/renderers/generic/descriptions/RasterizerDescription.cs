using System;

namespace odl3d.Renderer;

public sealed record RasterizerDescription
{
    public FillMode FillMode { get; init; } = FillMode.Solid;
    public CullMode CullMode { get; init; } = CullMode.None;
    public FrontFace FrontFace { get; init; } = FrontFace.CounterClockwise;
    public bool DepthClip { get; init; } = true;
}