using System;
using System.Numerics;

namespace odl3d.Renderer;

public interface IRenderPass : IDisposable
{
    void SetViewport(Rect viewportRect);
    void SetScissor(Rect scissorRect);

    void SetRenderPipeline(IRenderPipeline shaderPipeline);
    void SetDepthStencilState(IDepthStencilState state);

    void SetVertexBuffer<T>(IBuffer<T> buffer, int slot = 0, nuint offset = 0) where T : unmanaged;
    void SetIndexBuffer<T>(IBuffer<T> buffer, nuint offset = 0) where T : unmanaged;
    void SetUniformBuffer<T>(int slot, IBuffer<T> buffer, nuint offset = 0) where T : unmanaged;

    void SetTexture(int slot, ITexture texture);
    void SetSampler(int slot, ISampler sampler);

    void Draw(int startIndex, int vertexCount);
    void Draw();
    void DrawIndexed(int startIndex, int indexCount);
    void DrawIndexed();

    void End();
}