using System;
using System.Numerics;

namespace odl3d.Renderer;

public interface IRenderPass : IDisposable
{
    void SetViewport(Rect viewportRect);
    void SetScissor(Rect scissorRect);

    void SetRenderPipeline(IRenderPipeline shaderPipeline);
    void SetDepthStencilState(IDepthStencilState state);

    void SetVertexBuffer<T>(IBuffer<T> buffer, int slot = 0, uint offset = 0) where T : unmanaged;
    void SetIndexBuffer<T>(IBuffer<T> buffer) where T : unmanaged;
    void SetUniformBuffer<T>(IBuffer<T> buffer, int slot = 0, uint offset = 0) where T : unmanaged;

    void SetTexture(ITexture texture, int slot = 0);
    void SetSampler(ISampler sampler, int slot = 0);

    void Draw(int startIndex, int vertexCount);
    void DrawIndexed(int startIndex, int indexCount);
    void DrawIndexed();

    void End();
}