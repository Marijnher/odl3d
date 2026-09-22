using System;

namespace odl3d.Renderer.MetalAdapter;

public class MetalRenderPass : IRenderPass
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
    private Metal.PrimitiveType MetalPrimitiveType;
    private MetalTexture? Texture;
    private MetalSampler? Sampler;

    public bool Disposed { get; private set; }

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
        colAtch0.SetTexture(Drawable.Texture);
        colAtch0.SetLoadAction(description.Color.LoadAction);
        colAtch0.SetStoreAction(description.Color.StoreAction);
        colAtch0.SetClearColor(description.Color.ClearColor);

        Metal.RenderPassDepthAttachment depthAttachment = renderPass.DepthAttachment;
        depthAttachment.SetTexture(RenderSurface.DepthTexture);
        depthAttachment.SetLoadAction(description.Depth?.LoadAction ?? LoadAction.Clear);
        depthAttachment.SetStoreAction(description.Depth?.StoreAction ?? StoreAction.DontCare);
        depthAttachment.SetClearDepth(description.Depth?.ClearDepth ?? 1.0);

        Encoder = CommandBuffer.CreateRenderCommandEncoder(renderPass);
    }

    public void SetViewport(Rect viewportRect) => 
        Encoder.SetViewport(viewportRect.X, viewportRect.Y, viewportRect.Width, viewportRect.Height);
    public void SetScissor(Rect scissorRect) =>
        Encoder.SetScissorRect((nuint) scissorRect.X, (nuint) scissorRect.Y, (nuint) scissorRect.Width, (nuint) scissorRect.Height);

    public void SetRenderPipeline(IRenderPipeline shaderPipeline)
    {
        Pipeline = (MetalRenderPipeline) shaderPipeline;
        Encoder.SetRenderPipelineState(Pipeline.Pipeline);
        MetalPrimitiveType = Pipeline.PrimitiveType switch
        {
            Renderer.PrimitiveType.LineList => Metal.PrimitiveType.Line,
            Renderer.PrimitiveType.LineStrip => Metal.PrimitiveType.LineStrip,
            Renderer.PrimitiveType.TriangleList => Metal.PrimitiveType.Triangle,
            Renderer.PrimitiveType.TriangleStrip => Metal.PrimitiveType.TriangleStrip,
            _ => throw new RenderException($"Unsupported primitive type: {Pipeline.PrimitiveType}")
        };
    }
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

    public void SetTexture(ITexture texture, uint slot = 0)
    {
        Texture = (MetalTexture) texture;
        Encoder.SetFragmentTexture(Texture.Texture, slot);
    }
    public void SetSampler(ISampler sampler, uint slot = 0)
    {
        Sampler = (MetalSampler) sampler;
        Encoder.SetFragmentSamplerState(Sampler.Sampler, slot);
    }

    void PreDraw()
    {
        if (Texture != null && Sampler != null && Sampler.MipmapFilter != MipmapFilter.None)
        {
            Texture.ValidateMipmaps(Device, CommandQueue);
        }
    }

    public void Draw(int startIndex, int vertexCount)
    {
        if (Pipeline == null) throw new RenderException("Cannot draw without a valid pipeline attached.");
        PreDraw();
        Encoder.DrawPrimitives(MetalPrimitiveType, startIndex, vertexCount);
    }

    public void DrawIndexed(int startIndex, int indexCount)
    {
        if (VertexBuffer == null) throw new RenderException("Cannot draw without a valid vertex buffer attached.");
        if (IndexBuffer == null) throw new RenderException("Cannot draw without a valid index buffer attached.");
        PreDraw();
        Encoder.DrawIndexedPrimitives(
            indexBuffer: IndexBuffer.Buffer,
            indexCount: (uint) indexCount,
            indexType: Metal.IndexType.UInt32,
            indexBufferOffset: (nuint) startIndex * sizeof(uint),
            primitiveType: MetalPrimitiveType
        );
    }
    public void DrawIndexed()
    {
        if (VertexBuffer == null) throw new RenderException("Cannot draw without a valid vertex buffer attached.");
        if (IndexBuffer == null) throw new RenderException("Cannot draw without a valid index buffer attached.");
        DrawIndexed(0, IndexBuffer.Size);
    }

    public void End() => Encoder.EndEncoding();

    public void Dispose() 
    {
        if (Disposed) return;
        Encoder.Dispose();
        Disposed = true;
    }
}