using System;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a Metal depth-stencil state, encapsulating depth test and write settings along with the depth compare function.
/// </summary>
internal class MetalDepthStencilState : IDepthStencilState
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
    /// Gets the depth compare function used by the depth-stencil state.
    /// </summary>
    public CompareFunction DepthCompareFunction { get; }

    /// <summary>
    /// Gets a value indicating whether the depth-stencil state has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Gets the underlying Metal depth-stencil state object.
    /// </summary>
    public Metal.DepthStencilState DepthStencilState;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalDepthStencilState"/> class with the specified device and depth-stencil description.
    /// </summary>
    /// <param name="device">The Metal device used to create the depth-stencil state.</param>
    /// <param name="description">The description of the depth-stencil state.</param>
    public MetalDepthStencilState(Metal.Device device, DepthStencilDescription description)
    {
        DepthTestEnabled = description.DepthTestEnabled;
        DepthWriteEnabled = description.DepthWriteEnabled;
        DepthCompareFunction = description.DepthCompareFunction;
        DepthStencilState = device.CreateDepthStencilState(
            description.DepthCompareFunction,
            description.DepthWriteEnabled
        );
    }

    /// <summary>
    /// Disposes the depth-stencil state, releasing its underlying Metal resources.
    /// </summary>
    public void Dispose() 
    {
        if (Disposed) return;
        DepthStencilState.Dispose();
        Disposed = true;
    }
}