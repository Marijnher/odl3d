using System;

namespace odl3d.Renderer;

public sealed record BlendDescription
{
    public bool Enabled { get; init; }

    public BlendFactor SourceColor { get; init; } =
        BlendFactor.One;

    public BlendFactor DestinationColor { get; init; } =
        BlendFactor.Zero;

    public BlendOperation ColorOperation { get; init; } =
        BlendOperation.Add;

    public BlendFactor SourceAlpha { get; init; } =
        BlendFactor.One;

    public BlendFactor DestinationAlpha { get; init; } =
        BlendFactor.Zero;

    public BlendOperation AlphaOperation { get; init; } =
        BlendOperation.Add;
}