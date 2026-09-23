using System;
using System.Numerics;

namespace odl3d.Renderer;

/// <summary>
/// Represents a render pass in the rendering pipeline, providing methods to set various pipeline states, bind resources, and issue draw calls.
/// </summary>
public interface IRenderPass : IDisposable
{
    /// <summary>
    /// Sets the viewport for the render pass.
    /// </summary>
    /// <param name="viewportRect">The rectangle defining the viewport.</param>
    void SetViewport(Rect viewportRect);

    /// <summary>
    /// Sets the scissor rectangle for the render pass.
    /// </summary>
    /// <param name="scissorRect">The rectangle defining the scissor area.</param>
    void SetScissor(Rect scissorRect);

    /// <summary>
    /// Sets the render pipeline for the render pass.
    /// </summary>
    /// <param name="shaderPipeline">The render pipeline to set for the render pass.</param>
    void SetRenderPipeline(IRenderPipeline shaderPipeline);

    /// <summary>
    /// Sets the depth-stencil state for the render pass.
    /// </summary>
    /// <param name="state">The depth-stencil state to set for the render pass.</param>
    void SetDepthStencilState(IDepthStencilState state);

    /// <summary>
    /// Sets the vertex buffer for the render pass.
    /// </summary>
    /// <typeparam name="T">The type of the vertex data.</typeparam>
    /// <param name="buffer">The vertex buffer to set for the render pass.</param>
    /// <param name="slot">The slot index to bind the vertex buffer to.</param>
    /// <param name="offset">The offset within the vertex buffer.</param>
    void SetVertexBuffer<T>(IBuffer<T> buffer, int slot = 0, uint offset = 0) where T : unmanaged;

    /// <summary>
    /// Sets the index buffer for the render pass.
    /// </summary>
    /// <typeparam name="T">The type of the index data.</typeparam>
    /// <param name="buffer">The index buffer to set for the render pass.</param>
    void SetIndexBuffer<T>(IBuffer<T> buffer) where T : unmanaged;

    /// <summary>
    /// Sets the uniform buffer for the render pass.
    /// </summary>
    /// <typeparam name="T">The type of the uniform data.</typeparam>
    /// <param name="buffer">The uniform buffer to set for the render pass.</param>
    /// <param name="slot">The slot index to bind the uniform buffer to.</param>
    /// <param name="offset">The offset within the uniform buffer.</param>
    void SetUniformBuffer<T>(IBuffer<T> buffer, int slot = 0, uint offset = 0) where T : unmanaged;

    /// <summary>
    /// Sets the texture for the render pass.
    /// </summary>
    /// <param name="texture">The texture to set for the render pass.</param>
    /// <param name="slot">The slot index to bind the texture to.</param>
    void SetTexture(ITexture texture, uint slot = 0);

    /// <summary>
    /// Sets the sampler for the render pass.
    /// </summary>
    /// <param name="sampler">The sampler to set for the render pass.</param>
    /// <param name="slot">The slot index to bind the sampler to.</param>
    void SetSampler(ISampler sampler, uint slot = 0);

    /// <summary>
    /// Draws non-indexed primitives for the render pass.
    /// </summary>
    /// <param name="startIndex">The starting index of the vertices to draw.</param>
    /// <param name="vertexCount">The number of vertices to draw.</param>
    void Draw(int startIndex, int vertexCount);

    /// <summary>
    /// Draws indexed primitives for the render pass.
    /// </summary>
    /// <param name="startIndex">The starting index of the indices to draw.</param>
    /// <param name="indexCount">The number of indices to draw.</param>
    void DrawIndexed(int startIndex, int indexCount);

    /// <summary>
    /// Draws all indexed primitives for the render pass.
    /// </summary>
    void DrawIndexed();

    /// <summary>
    /// Ends the render pass and commits this render pass to the render frame.
    /// </summary>
    void End();
}