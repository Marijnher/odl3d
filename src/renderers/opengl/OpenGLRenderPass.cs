using System;
using System.Runtime.CompilerServices;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL render pass, managing the state and resources required for rendering operations.
/// </summary>
internal class OpenGLRenderPass : IRenderPass
{
    private OpenGLRenderSurface RenderSurface { get; }

    /// <summary>
    /// Gets a value indicating whether the render pass has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    private VertexSlot[] _vbSlots = new VertexSlot[GLVertexLayout.MaxSlots];
    private OpenGLRenderPipeline? _renderPipeline;
    private GLVertexState _vertexState;
    private bool _vertexStateDirty;
    private uint _indexBufferHandle;
    uint _indexType;
    int _indexSize;
    int _indexCount;

    private OpenGLTexture? Texture;
    private OpenGLSampler? Sampler;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLRenderPass"/> class.
    /// </summary>
    /// <param name="renderSurface">The render surface associated with this render pass.</param>
    /// <param name="vertexState">The initial vertex state for the render pass.</param>
    /// <param name="description">The description of the render pass.</param>
    public OpenGLRenderPass(OpenGLRenderSurface renderSurface, GLVertexState vertexState, RenderPassDescription description)
    {
        RenderSurface = renderSurface;
        _vertexState = vertexState;
        uint clearFlags = 0;
        GL.glDisable(GL.GL_SCISSOR_TEST);
        if (description.Color.LoadAction == LoadAction.Clear)
        {
            var col = description.Color.ClearColor.ToVector4();
            GL.glClearColor(col.X, col.Y, col.Z, col.W);
            clearFlags |= GL.GL_COLOR_BUFFER_BIT;
        }
        if (description.Depth == null || description.Depth.LoadAction == LoadAction.Clear)
        {
            GL.glDepthMask(GL.GL_TRUE);
            GL.glClearDepth(description.Depth?.ClearDepth ?? 1.0);
            clearFlags |= GL.GL_DEPTH_BUFFER_BIT;
        }
        if (clearFlags != 0) GL.glClear(clearFlags);
    }

    /// <summary>
    /// Sets the viewport for the render pass.
    /// </summary>
    /// <param name="viewportRect">The rectangle defining the viewport dimensions and position.</param>
    public void SetViewport(Rect viewportRect)
    {
        float y = RenderSurface.Height - (viewportRect.Y + viewportRect.Height);
        GL.glViewport((int) viewportRect.X, (int) y, (int) viewportRect.Width, (int) viewportRect.Height);
    }

    /// <summary>
    /// Sets the scissor rectangle for the render pass.
    /// </summary>
    /// <param name="scissorRect">The rectangle defining the scissor area dimensions and position.</param>
    public void SetScissor(Rect scissorRect)
    {
        GL.glEnable(GL.GL_SCISSOR_TEST);
        float y = RenderSurface.Height - (scissorRect.Y + scissorRect.Height);
        GL.glScissor((int) scissorRect.X, (int) y, (int) scissorRect.Width, (int) scissorRect.Height);
    }

    /// <summary>
    /// Sets the render pipeline for the render pass.
    /// </summary>
    /// <param name="shaderPipeline">The render pipeline to set for the render pass.</param>
    public void SetRenderPipeline(IRenderPipeline shaderPipeline)
    {
        _renderPipeline = (OpenGLRenderPipeline) shaderPipeline;
        _renderPipeline.Use();
        _vertexStateDirty = true;
    }

    /// <summary>
    /// Sets the depth-stencil state for the render pass.
    /// </summary>
    /// <param name="state">The depth-stencil state to set for the render pass.</param>
    public void SetDepthStencilState(IDepthStencilState state)
    {
        if (state.DepthTestEnabled || state.DepthWriteEnabled) GL.glEnable(GL.GL_DEPTH_TEST);
        else GL.glDisable(GL.GL_DEPTH_TEST);

        GL.glDepthMask(state.DepthWriteEnabled ? GL.GL_TRUE : GL.GL_FALSE);

        GL.glDepthFunc(state.DepthTestEnabled
            ? GetDepthFunction(state.DepthCompareFunction)
            : GL.GL_ALWAYS);
    }

    /// <summary>
    /// Sets the vertex buffer for the render pass.
    /// </summary>
    /// <typeparam name="T">The type of the vertex buffer elements.</typeparam>
    /// <param name="buffer">The vertex buffer to set for the render pass.</param>
    /// <param name="slot">The slot index to bind the vertex buffer to.</param>
    /// <param name="offset">The offset within the vertex buffer.</param>
    public void SetVertexBuffer<T>(IBuffer<T> buffer, int slot = 0, uint offset = 0) where T : unmanaged
    {
        var b = (OpenGLBuffer<T>) buffer;
        _vbSlots[slot] = new VertexSlot { ID = b.Handle, Offset = offset };
        _vertexStateDirty = true;
    }
    
    /// <summary>
    /// Sets the index buffer for the render pass.
    /// </summary>
    /// <typeparam name="T">The type of the index buffer elements.</typeparam>
    /// <param name="buffer">The index buffer to set for the render pass.</param>
    /// <exception cref="RenderException">Thrown if the buffer is not an index buffer or if the type is unsupported.</exception>
    public unsafe void SetIndexBuffer<T>(IBuffer<T> buffer) where T : unmanaged
    {
        var b = (OpenGLBuffer<T>) buffer;
        if (b.Usage != BufferUsage.Index) throw new RenderException($"Buffer is not an index buffer.");

        _indexBufferHandle = b.Handle;
        _indexType = typeof(T) switch
        {
            _ when typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte) => GL.GL_UNSIGNED_BYTE,
            _ when typeof(T) == typeof(ushort) || typeof(T) == typeof(short) => GL.GL_UNSIGNED_SHORT,
            _ when typeof(T) == typeof(uint) || typeof(T) == typeof(int) => GL.GL_UNSIGNED_INT,
            _ => throw new RenderException($"Unsupported index buffer type {typeof(T)}.")
        };
        _indexSize = sizeof(T);
        _indexCount = b.Size;
    }

    /// <summary>
    /// Sets the uniform buffer for the render pass.
    /// </summary>
    /// <typeparam name="T">The type of the uniform buffer elements.</typeparam>
    /// <param name="buffer">The uniform buffer to set for the render pass.</param>
    /// <param name="slot">The slot index to bind the uniform buffer to.</param>
    /// <param name="offset">The offset within the uniform buffer.</param>
    /// <exception cref="RenderException">Thrown if the buffer is not a uniform buffer or if the offset is invalid.</exception>
    public unsafe void SetUniformBuffer<T>(IBuffer<T> buffer, int slot = 0, uint offset = 0) where T : unmanaged
    {
        if ((uint) slot >= BufferSlots.MaxUniformBuffers)
            throw new RenderException($"Uniform slot must be 0..{BufferSlots.MaxUniformBuffers - 1}.");

        var b = (OpenGLBuffer<T>) buffer;
        if (b.Usage != BufferUsage.Uniform)
            throw new RenderException("Buffer is not a uniform buffer.");
        if (offset % BufferSlots.UniformOffsetAlignment != 0)
            throw new RenderException(
                $"Uniform buffer offset {offset} must be a multiple of {BufferSlots.UniformOffsetAlignment}.");

        nint totalBytes = b.Size * sizeof(T);
        nint size = Math.Min(sizeof(T), totalBytes - (nint) offset);
        GL.glBindBufferRange(GL.GL_UNIFORM_BUFFER, (uint) slot, b.Handle, (nint) offset, size);
    }

    /// <summary>
    /// Sets the texture for the render pass.
    /// </summary>
    /// <param name="texture">The texture to set for the render pass.</param>
    /// <param name="slot">The texture slot to bind the texture to.</param>
    public void SetTexture(ITexture texture, uint slot = 0)
    {
        Texture = (OpenGLTexture) texture;
        GL.glActiveTexture(GL.GL_TEXTURE0 + slot);
        GL.glBindTexture(GL.GL_TEXTURE_2D, Texture.Handle);
    }

    /// <summary>
    /// Sets the sampler for the render pass.
    /// </summary>
    /// <param name="sampler">The sampler to set for the render pass.</param>
    /// <param name="slot">The sampler slot to bind the sampler to.</param>
    public void SetSampler(ISampler sampler, uint slot = 0)
    {
        Sampler = (OpenGLSampler) sampler;
        GL.glBindSampler(slot, Sampler.Handle);
    }

    /// <summary>
    /// Prepares the render pass for drawing by validating mipmaps if necessary.
    /// </summary>
    void PreDraw()
    {
        if (Texture != null && Sampler != null && Sampler.MipmapFilter != MipmapFilter.None)
        {
            Texture.ValidateMipmaps();
        }
    }

    /// <summary>
    /// Draws non-indexed primitives using the specified start index and vertex count.
    /// </summary>
    /// <param name="startIndex">The starting index of the vertices to draw.</param>
    /// <param name="vertexCount">The number of vertices to draw.</param>
    public void Draw(int startIndex, int vertexCount)
    {
        FlushVertexState();
        PreDraw();
        PrimitiveType primitiveType = _renderPipeline!.Wireframe ? PrimitiveType.LineStrip : _renderPipeline!.PrimitiveType;
        GL.glDrawArrays(GetPrimitiveType(primitiveType), startIndex, vertexCount);
    }

    /// <summary>
    /// Draws indexed primitives using the specified start index and index count.
    /// </summary>
    /// <param name="startIndex">The starting index of the indices to draw.</param>
    /// <param name="indexCount">The number of indices to draw.</param>
    public void DrawIndexed(int startIndex, int indexCount)
    {
        FlushVertexState();
        _vertexState.BindIndexBuffer(_indexBufferHandle);
        PreDraw();
        PrimitiveType primitiveType = _renderPipeline!.Wireframe ? PrimitiveType.LineStrip : _renderPipeline!.PrimitiveType;
        GL.glDrawElements(GetPrimitiveType(primitiveType), indexCount, _indexType,
                        startIndex * _indexSize);
    }

    /// <summary>
    /// Draws all indexed primitives using the default start index and the total index count.
    /// </summary>
    public void DrawIndexed() => DrawIndexed(0, _indexCount);

    /// <summary>
    /// Flushes the current vertex state to ensure it is up-to-date before drawing.
    /// </summary>
    private void FlushVertexState()
    {
        if (!_vertexStateDirty) return;
        _vertexState.Apply(_renderPipeline!.GLLayout, _vbSlots);
        _vertexStateDirty = false;
    }

    /// <summary>
    /// Ends the current render pass by unbinding resources and resetting the OpenGL state.
    /// </summary>
    public void End()
    {
        GL.glDisable(GL.GL_BLEND);
        GL.glBindTexture(GL.GL_TEXTURE_2D, 0);
        GL.glBindSampler(0, 0);
        GL.glBindBuffer(GL.GL_UNIFORM_BUFFER, 0);
        GL.glBindBuffer(GL.GL_ARRAY_BUFFER, 0);
        GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, 0);
        GL.glUseProgram(0);
    }

    /// <summary>
    /// Disposes of the render pass, releasing any associated resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
    }

    private uint GetPrimitiveType(PrimitiveType primitiveType) => primitiveType switch
    {
        PrimitiveType.TriangleList => GL.GL_TRIANGLES,
        PrimitiveType.TriangleStrip => GL.GL_TRIANGLE_STRIP,
        PrimitiveType.LineList => GL.GL_LINES,
        PrimitiveType.LineStrip => GL.GL_LINE_STRIP,
        _ => throw new ArgumentOutOfRangeException(nameof(primitiveType), primitiveType, null)
    };

    private uint GetDepthFunction(CompareFunction depthFunction) => depthFunction switch
    {
        CompareFunction.Never => GL.GL_NEVER,
        CompareFunction.Less => GL.GL_LESS,
        CompareFunction.Equal => GL.GL_EQUAL,
        CompareFunction.LessEqual => GL.GL_LEQUAL,
        CompareFunction.Greater => GL.GL_GREATER,
        CompareFunction.NotEqual => GL.GL_NOTEQUAL,
        CompareFunction.GreaterEqual => GL.GL_GEQUAL,
        CompareFunction.Always => GL.GL_ALWAYS,
        _ => throw new ArgumentOutOfRangeException(nameof(depthFunction), depthFunction, null)
    };
}