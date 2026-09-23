using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents the rendering capabilities of an OpenGL renderer.
/// </summary>
internal class OpenGLRenderCapabilities : IRenderCapabilities
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
    /// Gets the maximum texture size supported by the renderer.
    /// </summary>

    public int MaxTextureSize => throw new NotImplementedException();
    
    /// <summary>
    /// Gets the maximum anisotropy level supported by the renderer.
    /// </summary>
    public int MaxAnisotropy => throw new NotImplementedException();
    
    /// <summary>
    /// Gets the maximum number of vertex buffer slots supported by the renderer.
    /// </summary>
    public int MaxVertexBufferSlots => throw new NotImplementedException();
    
    /// <summary>
    /// Gets the maximum number of texture slots supported by the renderer.
    /// </summary>
    public int MaxTextureSlots => throw new NotImplementedException();
}