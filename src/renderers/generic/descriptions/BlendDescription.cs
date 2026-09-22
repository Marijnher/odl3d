using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the blend state configuration for a rendering pipeline, including color and alpha blending factors and operations.
/// </summary>
public sealed record BlendDescription
{
    /// <summary>
    /// Indicates whether blending is enabled for the rendering pipeline.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// The blend factor used for the source color in the blending operation.
    /// </summary>
    public BlendFactor SourceColor { get; init; } = BlendFactor.One;

    /// <summary>
    /// The blend factor used for the destination color in the blending operation.
    /// </summary>
    public BlendFactor DestinationColor { get; init; } = BlendFactor.Zero;

    /// <summary>
    /// The blend operation used for the color channels.
    /// </summary>
    public BlendOperation ColorOperation { get; init; } = BlendOperation.Add;

    /// <summary>
    /// The blend factor used for the source alpha in the blending operation.
    /// </summary>
    public BlendFactor SourceAlpha { get; init; } = BlendFactor.One;

    /// <summary>
    /// The blend factor used for the destination alpha in the blending operation.
    /// </summary>
    public BlendFactor DestinationAlpha { get; init; } = BlendFactor.Zero;

    /// <summary>
    /// The blend operation used for the alpha channel.
    /// </summary>
    public BlendOperation AlphaOperation { get; init; } = BlendOperation.Add;
}