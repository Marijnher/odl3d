using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the rendering capabilities of the GPU, indicating which features are supported and the maximum limits for various resources.
/// </summary>
public interface IRenderCapabilities
{
    /// <summary>
    /// Gets a value indicating whether wireframe rendering is supported by the GPU.
    /// </summary>
    bool SupportsWireframe { get; }

    /// <summary>
    /// Gets a value indicating whether compute shaders are supported by the GPU.
    /// </summary>
    bool SupportsComputeShaders { get; }

    /// <summary>
    /// Gets a value indicating whether geometry shaders are supported by the GPU.
    /// </summary>
    bool SupportsGeometryShaders { get; }

    /// <summary>
    /// Gets a value indicating whether anisotropic filtering is supported by the GPU.
    /// </summary>
    bool SupportsAnisotropicFiltering { get; }

    /// <summary>
    /// Gets a value indicating whether independent blend is supported by the GPU.
    /// </summary>
    bool SupportsIndependentBlend { get; }

    /// <summary>
    /// Gets a value indicating whether indirect draw is supported by the GPU.
    /// </summary>
    bool SupportsIndirectDraw { get; }

    /// <summary>
    /// Gets the maximum supported texture size by the GPU.
    /// </summary>
    int MaxTextureSize { get; }

    /// <summary>
    /// Gets the maximum supported anisotropy level by the GPU.
    /// </summary>
    int MaxAnisotropy { get; }

    /// <summary>
    /// Gets the maximum number of vertex buffer slots supported by the GPU.
    /// </summary>
    int MaxVertexBufferSlots { get; }

    /// <summary>
    /// Gets the maximum number of texture slots supported by the GPU.
    /// </summary>
    int MaxTextureSlots { get; }
}