using System;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a render pass in the Metal rendering pipeline.
/// </summary>
internal class MetalRenderPass : IRenderPass
{
    private Metal.Device Device;
    private MetalRenderSurface RenderSurface;
    private Metal.CommandQueue CommandQueue;
    private Metal.Drawable Drawable;
    private Metal.CommandBuffer CommandBuffer;
    private Metal.CommandEncoder Encoder;

    private MetalRenderPipeline? Pipeline;
    private MetalBuffer? VertexBuffer;
    private MetalBuffer? IndexBuffer;
    private MetalTexture? Texture;
    private MetalSampler? Sampler;

    /// <summary>
    /// Gets a value indicating whether the render pass has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalRenderPass"/> class.
    /// </summary>
    /// <param name="device">The Metal device associated with the render pass.</param>
    /// <param name="renderSurface">The render surface associated with the render pass.</param>
    /// <param name="commandQueue">The command queue used for issuing rendering commands.</param>
    /// <param name="drawable">The drawable representing the render target.</param>
    /// <param name="commandBuffer">The command buffer used for encoding rendering commands.</param>
    /// <param name="description">The description of the render pass to create.</param>
    public MetalRenderPass(
        Metal.Device device,
        MetalRenderSurface renderSurface,
        Metal.CommandQueue commandQueue,
        Metal.Drawable drawable,
        Metal.CommandBuffer commandBuffer,
        RenderPassDescription description)
    {
        Device = device;
        RenderSurface = renderSurface;
        CommandQueue = commandQueue;
        Drawable = drawable;
        CommandBuffer = commandBuffer;

        using var renderPass = Device.NewRenderPassDescriptor();

        Metal.RenderPassColorAttachment colAtch0 = renderPass.ColorAttachments[0];
        colAtch0.Texture = Drawable.Texture;
        colAtch0.LoadAction = description.Color.LoadAction;
        colAtch0.StoreAction = description.Color.StoreAction;
        colAtch0.SetClearColor(description.Color.ClearColor);

        Metal.RenderPassDepthAttachment depthAttachment = renderPass.DepthAttachment;
        depthAttachment.Texture = RenderSurface.DepthTexture;
        depthAttachment.LoadAction = description.Depth?.LoadAction ?? LoadAction.Clear;
        depthAttachment.StoreAction = description.Depth?.StoreAction ?? StoreAction.DontCare;
        depthAttachment.ClearDepth = description.Depth?.ClearDepth ?? 1.0;

        Encoder = CommandBuffer.CreateRenderCommandEncoder(renderPass);
    }

    /// <summary>
    /// Sets the viewport for the render pass.
    /// </summary>
    /// <param name="viewportRect">The rectangle defining the viewport dimensions.</param>
    public void SetViewport(Rect viewportRect) => 
        Encoder.SetViewport(viewportRect.X, viewportRect.Y, viewportRect.Width, viewportRect.Height);

    /// <summary>
    /// Sets the scissor rectangle for the render pass.
    /// </summary>
    /// <param name="scissorRect">The rectangle defining the scissor dimensions.</param>
    public void SetScissor(Rect scissorRect) =>
        Encoder.SetScissorRect((nuint) scissorRect.X, (nuint) scissorRect.Y, (nuint) scissorRect.Width, (nuint) scissorRect.Height);

    /// <summary>
    /// Sets the render pipeline for the render pass.
    /// </summary>
    /// <param name="shaderPipeline">The render pipeline to set for the render pass.</param>
    public void SetRenderPipeline(IRenderPipeline shaderPipeline)
    {
        Pipeline = (MetalRenderPipeline) shaderPipeline;
        Encoder.SetRenderPipelineState(Pipeline.Pipeline);
    }

    /// <summary>
    /// Sets the depth stencil state for the render pass.
    /// </summary>
    /// <param name="state">The depth stencil state to set for the render pass.</param>
    public void SetDepthStencilState(IDepthStencilState state)
    {
        var metalDepthStencil = (MetalDepthStencilState) state;
        Encoder.SetDepthStencilState(metalDepthStencil.DepthStencilState);
    }

    /// <summary>
    /// Sets the vertex buffer for the render pass. Slots 0..3 map to 27..30 in the MSL shader.
    /// </summary>
    /// <typeparam name="T">The type of the vertex buffer elements.</typeparam>
    /// <param name="buffer">The vertex buffer to set.</param>
    /// <param name="slot">The slot to bind the vertex buffer to.</param>
    /// <param name="offset">The offset within the vertex buffer.</param>
    public void SetVertexBuffer<T>(IBuffer<T> buffer, int slot = 0, uint offset = 0) where T : unmanaged
    {
        VertexBuffer = (MetalBuffer<T>) buffer;
        Encoder.SetVertexBuffer(VertexBuffer.Buffer, GetVertexSlot(slot), offset);
    }

    private static uint GetVertexSlot(int slot)
    {
        return (uint) (31 - BufferSlots.MaxVertexBuffers + slot);
    }

    /// <summary>
    /// Sets the index buffer for the render pass.
    /// </summary>
    /// <typeparam name="T">The type of the index buffer elements.</typeparam>
    /// <param name="buffer">The index buffer to set.</param>
    public void SetIndexBuffer<T>(IBuffer<T> buffer) where T : unmanaged
    {
        IndexBuffer = (MetalBuffer<T>) buffer;
    }

    /// <summary>
    /// Sets the uniform buffer for the render pass. This binds the buffer to both the vertex and fragment stages. Slots 0..15 map to the corresponding slots in the MSL shader.
    /// </summary>
    /// <typeparam name="T">The type of the uniform buffer elements.</typeparam>
    /// <param name="buffer">The uniform buffer to set.</param>
    /// <param name="slot">The slot to bind the uniform buffer to.</param>
    /// <param name="offset">The offset within the uniform buffer.</param>
    public void SetUniformBuffer<T>(IBuffer<T> buffer, int slot = 0, uint offset = 0) where T : unmanaged
    {
        var buf = (MetalBuffer<T>) buffer;
        Encoder.SetVertexBuffer(buf.Buffer, (nuint) slot, offset);
        Encoder.SetFragmentBuffer(buf.Buffer, (nuint) slot, offset);
    }

    /// <summary>
    /// Sets the texture for the render pass.
    /// </summary>
    /// <param name="texture">The texture to set for the render pass.</param>
    /// <param name="slot">The slot to bind the texture to.</param>
    public void SetTexture(ITexture texture, uint slot = 0)
    {
        Texture = (MetalTexture) texture;
        Encoder.SetFragmentTexture(Texture.Texture, slot);
    }

    /// <summary>
    /// Sets the sampler for the render pass.
    /// </summary>
    /// <param name="sampler">The sampler to set for the render pass.</param>
    /// <param name="slot">The slot to bind the sampler to.</param>
    public void SetSampler(ISampler sampler, uint slot = 0)
    {
        Sampler = (MetalSampler) sampler;
        Encoder.SetFragmentSamplerState(Sampler.Sampler, slot);
    }

    /// <summary>
    /// Prepares the render pass for drawing by validating mipmaps if necessary.
    /// </summary>
    void PreDraw()
    {
        if (Texture != null && Sampler != null && Sampler.MipmapFilter != MipmapFilter.None)
        {
            Texture.ValidateMipmaps(Device, CommandQueue);
        }
    }

    /// <summary>
    /// Draws primitives using the specified start index and vertex count.
    /// </summary>
    /// <param name="startIndex">The starting index of the vertices to draw.</param>
    /// <param name="vertexCount">The number of vertices to draw.</param>
    /// <exception cref="RenderException"></exception>
    public void Draw(int startIndex, int vertexCount)
    {
        if (Pipeline == null) throw new RenderException("Cannot draw without a valid pipeline attached.");
        PreDraw();
        Encoder.DrawPrimitives(
            primitiveType: Pipeline.Wireframe ? PrimitiveType.LineStrip : Pipeline.PrimitiveType,
            vertexStart: startIndex,
            vertexCount: vertexCount
        );
    }

    /// <summary>
    /// Draws indexed primitives using the specified start index and index count.
    /// </summary>
    /// <param name="startIndex">The starting index of the indices to draw.</param>
    /// <param name="indexCount">The number of indices to draw.</param>
    /// <exception cref="RenderException"></exception>
    public void DrawIndexed(int startIndex, int indexCount)
    {
        if (VertexBuffer == null) throw new RenderException("Cannot draw without a valid vertex buffer attached.");
        if (IndexBuffer == null) throw new RenderException("Cannot draw without a valid index buffer attached.");
        if (Pipeline == null) throw new RenderException("Cannot draw without a valid pipeline attached.");
        PreDraw();
        Encoder.DrawIndexedPrimitives(
            indexBuffer: IndexBuffer.Buffer,
            indexCount: (uint) indexCount,
            indexType: IndexType.UInt32,
            indexBufferOffset: (nuint) startIndex * sizeof(uint),
            primitiveType: Pipeline.Wireframe ? PrimitiveType.LineStrip : Pipeline.PrimitiveType
        );
    }
    /// <summary>
    /// Draws all indexed primitives from the beginning of the index buffer.
    /// </summary>
    /// <exception cref="RenderException"></exception>
    public void DrawIndexed()
    {
        if (IndexBuffer == null) throw new RenderException("Cannot draw without a valid index buffer attached.");
        DrawIndexed(0, IndexBuffer.Size);
    }

    /// <summary>
    /// Ends the current render pass by ending the encoding of commands.
    /// </summary>
    public void End() => Encoder.EndEncoding();

    /// <summary>
    /// Disposes of the render pass and releases any associated resources.
    /// </summary>
    public void Dispose() 
    {
        if (Disposed) return;
        Encoder.Dispose();
        Disposed = true;
    }
}