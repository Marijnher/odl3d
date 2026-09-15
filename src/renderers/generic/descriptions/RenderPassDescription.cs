using System;
using System.Collections.Generic;

namespace odl3d.Renderer;

public sealed record RenderPassDescription
{
    public required ColorAttachmentDescription Color { get; init; }

    public DepthAttachmentDescription? Depth { get; init; }

    public IReadOnlyList<ColorAttachmentDescription>? AdditionalColors { get; init; }
}