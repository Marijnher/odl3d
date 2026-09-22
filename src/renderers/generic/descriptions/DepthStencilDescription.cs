using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the description of the depth and stencil state in a rendering pipeline, including depth test, depth write, and depth compare function.
/// </summary>
public sealed record DepthStencilDescription
{
    /// <summary>
    /// Indicates whether depth testing is enabled.
    /// </summary>
    public bool DepthTestEnabled { get; init; }

    /// <summary>
    /// Indicates whether depth writing is enabled.
    /// </summary>
    public bool DepthWriteEnabled { get; init; }

    /// <summary>
    /// The function used to compare depth values when depth testing is enabled.
    /// </summary>
    public CompareFunction DepthCompareFunction { get; init; } = CompareFunction.Less;
}