using System;

namespace odl3d.Renderer;

public sealed record RenderPipelineDescription
{
    public required IShaderModule VertexShader { get; init; }
    public required IShaderModule FragmentShader { get; init; }

    public required VertexLayoutDescription VertexLayout { get; init; }

    public required TextureFormat ColorFormat { get; init; }
    public TextureFormat? DepthFormat { get; init; }

    public PrimitiveType PrimitiveType { get; init; } = PrimitiveType.TriangleList;

    public int SampleCount { get; init; } = 1;

    public RasterizerDescription Rasterizer { get; init; } = new RasterizerDescription();
    public BlendDescription Blend { get; init; } = new BlendDescription();
    public DepthStencilDescription DepthStencil { get; init; } = new DepthStencilDescription();
}