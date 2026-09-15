using System;

namespace odl3d.Renderer;

public sealed record RenderPipelineDescription
{
    public required IShaderModule VertexShader { get; init; }
    public required IShaderModule FragmentShader { get; init; }

    public required VertexLayoutDescription VertexLayout { get; init; }

    public required TextureFormat ColorFormat { get; init; }
    public required TextureFormat? DepthFormat { get; init; }

    public RasterizerDescription Rasterizer { get; init; } = new RasterizerDescription();
    public BlendDescription Blend { get; init; } = new BlendDescription();
    public DepthStencilDescription DepthStencil { get; init; } = new DepthStencilDescription();
}