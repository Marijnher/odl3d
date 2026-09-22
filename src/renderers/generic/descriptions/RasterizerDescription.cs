using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the description of the rasterizer state in a rendering pipeline, including fill mode, cull mode, front face, and depth clipping.
/// </summary>
public sealed record RasterizerDescription
{
    /// <summary>
    /// The fill mode used by the rasterizer (e.g., solid or wireframe).
    /// </summary>
    public FillMode FillMode { get; init; } = FillMode.Solid;

    /// <summary>
    /// The cull mode used by the rasterizer (e.g., none, front, or back).
    /// </summary>
    public CullMode CullMode { get; init; } = CullMode.None;

    /// <summary>
    /// The front face winding order used by the rasterizer (e.g., clockwise or counter-clockwise).
    /// </summary>
    public FrontFace FrontFace { get; init; } = FrontFace.CounterClockwise;

    /// <summary>
    /// Indicates whether depth clipping is enabled.
    /// </summary>
    public bool DepthClip { get; init; } = true;
}