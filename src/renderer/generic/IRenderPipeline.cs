using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents a render pipeline that defines how rendering is performed, including the primitive type, vertex layout, and wireframe mode.
/// </summary>
public interface IRenderPipeline : IGPUResource
{
    /// <summary>
    /// Gets the primitive type used by the render pipeline.
    /// </summary>
    public PrimitiveType PrimitiveType { get; }

    /// <summary>
    /// Gets the vertex layout description used by the render pipeline.
    /// </summary>
    public VertexLayoutDescription VertexLayout { get; }

    /// <summary>
    /// Gets or sets a value indicating whether wireframe mode is enabled for the render pipeline.
    /// </summary>
    public bool Wireframe { get; set; }
}