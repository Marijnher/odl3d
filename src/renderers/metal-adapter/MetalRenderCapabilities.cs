using System;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents the rendering capabilities of the Metal graphics API.
/// </summary>
internal class MetalRenderCapabilities : IRenderCapabilities
{
    /// <summary>
    /// Gets a value indicating whether wireframe rendering is supported.
    /// </summary>
    public bool SupportsWireframe => throw new NotImplementedException();

    /// <summary>
    /// Gets a value indicating whether compute shaders are supported.
    /// </summary>
    public bool SupportsComputeShaders => throw new NotImplementedException();

    /// <summary>
    /// Gets a value indicating whether geometry shaders are supported.
    /// </summary>
    public bool SupportsGeometryShaders => throw new NotImplementedException();

    /// <summary>
    /// Gets a value indicating whether anisotropic filtering is supported.
    /// </summary>
    public bool SupportsAnisotropicFiltering => throw new NotImplementedException();

    /// <summary>
    /// Gets a value indicating whether independent blend is supported.
    /// </summary>
    public bool SupportsIndependentBlend => throw new NotImplementedException();

    /// <summary>
    /// Gets a value indicating whether indirect draw is supported.
    /// </summary>
    public bool SupportsIndirectDraw => throw new NotImplementedException();

    /// <summary>
    /// Gets the maximum supported texture size.
    /// </summary>
    public int MaxTextureSize => throw new NotImplementedException();

    /// <summary>
    /// Gets the maximum supported anisotropy level.
    /// </summary>
    public int MaxAnisotropy => throw new NotImplementedException();

    /// <summary>
    /// Gets the maximum number of vertex buffer slots.
    /// </summary>
    public int MaxVertexBufferSlots => throw new NotImplementedException();

    /// <summary>
    /// Gets the maximum number of texture slots.
    /// </summary>
    public int MaxTextureSlots => throw new NotImplementedException();
}