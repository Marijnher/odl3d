using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the depth and stencil state configuration for the GPU.
/// </summary>
public interface IDepthStencilState : IGPUResource
{
    /// <summary>
    /// Gets a value indicating whether depth testing is enabled.
    /// </summary>
    public bool DepthTestEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether depth writing is enabled.
    /// </summary>
    public bool DepthWriteEnabled { get; }

    /// <summary>
    /// Gets the comparison function used for depth testing.
    /// </summary>
    public CompareFunction DepthCompareFunction { get; }
}