using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the description of a render pipeline in a rendering system, including shaders, vertex layout, color and depth formats, rasterizer, blend, and depth-stencil states.
/// </summary>
public sealed record RenderPipelineDescription
{
    /// <summary>
    /// The vertex shader module used by the render pipeline.
    /// </summary>
    public required IShaderModule VertexShader { get; init; }
    /// <summary>
    /// The fragment shader module used by the render pipeline.
    /// </summary>
    public required IShaderModule FragmentShader { get; init; }

    /// <summary>
    /// The vertex layout description used by the render pipeline.
    /// </summary>
    public required VertexLayoutDescription VertexLayout { get; init; }

    /// <summary>
    /// The color format used by the render pipeline.
    /// </summary>
    public required TextureFormat ColorFormat { get; init; }

    /// <summary>
    /// The optional depth format used by the render pipeline.
    /// </summary>
    public TextureFormat? DepthFormat { get; init; }

    /// <summary>
    /// Indicates whether wireframe mode is enabled in the render pipeline.
    /// </summary>
    public bool Wireframe { get; init; }

    /// <summary>
    /// The primitive type used by the render pipeline (e.g., triangle list, line list).
    /// </summary>
    public PrimitiveType PrimitiveType { get; init; } = PrimitiveType.TriangleList;

    /// <summary>
    /// The sample count used for multi-sampling in the render pipeline.
    /// </summary>
    public int SampleCount { get; init; } = 1;

    /// <summary>
    /// The rasterizer state description used by the render pipeline.
    /// </summary>
    public RasterizerDescription Rasterizer { get; init; } = new RasterizerDescription();

    /// <summary>
    /// The blend state description used by the render pipeline.
    /// </summary>
    public BlendDescription Blend { get; init; } = new BlendDescription();

    /// <summary>
    /// The depth-stencil state description used by the render pipeline.
    /// </summary>
    public DepthStencilDescription DepthStencil { get; init; } = new DepthStencilDescription();
}