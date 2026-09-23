using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal command encoder, which is used to encode rendering commands for the GPU.
    /// </summary>
    public sealed class CommandEncoder : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the CommandEncoder class with the specified native handle.
        /// </summary>
        /// <param name="handle">The native handle of the Metal command encoder.</param>
        public CommandEncoder(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Issues a non-indexed draw using the currently configured render pipeline.
        /// </summary>
        /// <param name="primitiveType">The type of primitives to draw (e.g., triangle, line).</param>
        /// <param name="vertexStart">The starting index of the vertices to draw.</param>
        /// <param name="vertexCount">The number of vertices to draw.</param>
        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount) =>
            Send("drawPrimitives:vertexStart:vertexCount:", GetPrimitiveType(primitiveType), (nuint) vertexStart, (nuint) vertexCount);

        /// <summary>
        /// Issues an indexed draw using the currently configured render pipeline.
        /// </summary>
        /// <param name="indexBuffer">The buffer containing the indices for the draw call.</param>
        /// <param name="indexCount">The number of indices to draw. If null, the entire buffer length is used.</param>
        /// <param name="indexType">The type of indices (e.g., UInt16, UInt32).</param>
        /// <param name="indexBufferOffset">The offset within the index buffer to start reading indices from.</param>
        /// <param name="primitiveType">The type of primitives to draw (e.g., triangle, line).</param>
        public void DrawIndexedPrimitives(
                Buffer indexBuffer,
                uint? indexCount = null,
                IndexType indexType = IndexType.UInt32,
                nuint indexBufferOffset = 0,
                PrimitiveType primitiveType = PrimitiveType.TriangleList
            ) =>
            Send("drawIndexedPrimitives:indexCount:indexType:indexBuffer:indexBufferOffset:",
                GetPrimitiveType(primitiveType), indexCount ?? indexBuffer.Length, GetIndexType(indexType), indexBuffer.Handle, indexBufferOffset);

        /// <summary>
        /// Ends the encoding of commands for the current render pass. After calling this method, no further commands can be encoded using this command encoder.
        /// </summary>
        public void EndEncoding() => Send("endEncoding");

        /// <summary>
        /// Sets the viewport for the current render pass. The viewport defines the portion of the drawable to render to.
        /// </summary>
        /// <param name="x">The x-coordinate of the viewport's origin.</param>
        /// <param name="y">The y-coordinate of the viewport's origin.</param>
        /// <param name="width">The width of the viewport.</param>
        /// <param name="height">The height of the viewport.</param>
        public void SetViewport(double x, double y, double width, double height) =>
            Send("setViewport:", new MTLViewport { OriginX = x, OriginY = y, Width = width, Height = height, Zfar = 1.0, Znear = 0.0});

        /// <summary>
        /// Sets the scissor rectangle for the current render pass. The scissor rectangle defines the portion of the drawable that can be modified by rendering.
        /// </summary>
        /// <param name="x">The x-coordinate of the scissor rectangle's origin.</param>
        /// <param name="y">The y-coordinate of the scissor rectangle's origin.</param>
        /// <param name="width">The width of the scissor rectangle.</param>
        /// <param name="height">The height of the scissor rectangle.</param>
        public void SetScissorRect(nuint x, nuint y, nuint width, nuint height) =>
            Send("setScissorRect:", new MTLScissorRect { X = x, Y = y, Width = width, Height = height });

        /// <summary>
        /// Sets the render pipeline state for the current render pass. The render pipeline state defines the configuration of the graphics pipeline, including shaders, blending, and rasterization settings.
        /// </summary>
        /// <param name="pipeline">The render pipeline state to set.</param>
        public void SetRenderPipelineState(RenderPipelineState pipeline) =>
            Send("setRenderPipelineState:", pipeline);

        /// <summary>
        /// Sets the depth stencil state for the current render pass. The depth stencil state controls depth and stencil testing.
        /// </summary>
        /// <param name="state">The depth stencil state to set.</param>
        public void SetDepthStencilState(DepthStencilState state) =>
            Send("setDepthStencilState:", state);

        /// <summary>
        /// Sets the vertex buffer for the current render pass. The vertex buffer contains vertex data used by the vertex shader.
        /// </summary>
        /// <param name="buffer">The vertex buffer to set.</param>
        /// <param name="index">The index of the vertex buffer slot.</param>
        /// <param name="offset">The offset within the vertex buffer.</param>
        public void SetVertexBuffer(Buffer buffer, nuint index = 0, uint offset = 0) =>
            Send("setVertexBuffer:offset:atIndex:", buffer.Handle, offset, index);

        /// <summary>
        /// Sets the fragment buffer for the current render pass. The fragment buffer contains data used by the fragment shader.
        /// </summary>
        /// <param name="buffer">The fragment buffer to set.</param>
        /// <param name="index">The index of the fragment buffer slot.</param>
        /// <param name="offset">The offset within the fragment buffer.</param>
        public void SetFragmentBuffer(Buffer buffer, nuint index = 0, uint offset = 0) =>
            Send("setFragmentBuffer:offset:atIndex:", buffer.Handle, offset, index);

        /// <summary>
        /// Sets the fragment texture for the current render pass. The fragment texture is used by the fragment shader.
        /// </summary>
        /// <param name="texture">The fragment texture to set.</param>
        /// <param name="index">The index of the fragment texture slot.</param>
        public void SetFragmentTexture(Texture texture, nuint index = 0) =>
            Send("setFragmentTexture:atIndex:", texture.Handle, index);

        /// <summary>
        /// Sets the fragment sampler state for the current render pass. The fragment sampler state defines how textures are sampled in the fragment shader.
        /// </summary>
        /// <param name="sampler">The fragment sampler state to set.</param>
        /// <param name="index">The index of the fragment sampler state slot.</param>
        public void SetFragmentSamplerState(SamplerState sampler, nuint index = 0) =>
            Send("setFragmentSamplerState:atIndex:", sampler.Handle, index);
    }
}
