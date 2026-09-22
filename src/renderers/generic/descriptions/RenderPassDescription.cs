using System;
using System.Collections.Generic;

namespace odl3d.Renderer;

/// <summary>
/// Represents the description of a render pass in a rendering pipeline, including its primary color attachment, optional depth attachment, and additional color attachments.
/// </summary>
public sealed record RenderPassDescription
{
    /// <summary>
    /// The primary color attachment for the render pass.
    /// </summary>
    public required ColorAttachmentDescription Color { get; init; }

    /// <summary>
    /// The optional depth attachment for the render pass.
    /// </summary>
    public DepthAttachmentDescription? Depth { get; init; }

    /// <summary>
    /// Additional color attachments for the render pass.
    /// </summary>
    public IReadOnlyList<ColorAttachmentDescription>? AdditionalColors { get; init; }
}