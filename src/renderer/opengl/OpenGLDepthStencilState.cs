using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL depth-stencil state.
/// </summary>
internal class OpenGLDepthStencilState : IDepthStencilState
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

    /// <summary>
    /// Gets a value indicating whether the depth-stencil state has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLDepthStencilState"/> class.
    /// </summary>
    /// <param name="description">The description of the depth-stencil state.</param>
    public OpenGLDepthStencilState(DepthStencilDescription description)
    {
        DepthTestEnabled = description.DepthTestEnabled;
        DepthWriteEnabled = description.DepthWriteEnabled;
        DepthCompareFunction = description.DepthCompareFunction;
    }

    /// <summary>
    /// Disposes the depth-stencil state.
    /// </summary>
    public void Dispose() 
    {
        if (Disposed) return;
        Disposed = true;
    }
}