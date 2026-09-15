using System;
using System.Numerics;

namespace odl3d.Renderer;

public interface IRenderPass : IDisposable
{
    void SetViewport(Rect viewportRect);
    void SetScissor(Rect scissorRect);

    void SetRenderPipeline(IRenderPipeline shaderPipeline);
    void SetDepthStencilState(IDepthStencilState state);

    void SetVertexBuffer(int slot, IBuffer buffer, nuint offset = 0);
    void SetIndexBuffer(IBuffer buffer, IndexFormat format, nuint offset = 0);
    void SetUniformBuffer(int slot, IBuffer buffer, nuint offset = 0);

    void SetTexture(int slot, ITexture texture);
    void SetSampler(int slot, ISampler sampler);

    void Draw(int startIndex, int vertexCount);
    void DrawIndexed(int startIndex, int indexCount);

    void End();
}