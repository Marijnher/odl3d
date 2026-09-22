using System;
using System.Runtime.CompilerServices;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLRenderPass : IRenderPass
{
    private OpenGLRenderSurface RenderSurface { get; }

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
        if (description.Depth?.LoadAction == LoadAction.Clear)
        {
            GL.glDepthMask(GL.GL_TRUE);
            GL.glClearDepth(description.Depth.ClearDepth);
            clearFlags |= GL.GL_DEPTH_BUFFER_BIT;
        }
        if (clearFlags != 0) GL.glClear(clearFlags);
    }

    public void SetViewport(Rect viewportRect)
    {
        float y = RenderSurface.Height - (viewportRect.Y + viewportRect.Height);
        GL.glViewport((int) viewportRect.X, (int) y, (int) viewportRect.Width, (int) viewportRect.Height);
    }
    public void SetScissor(Rect scissorRect)
    {
        GL.glEnable(GL.GL_SCISSOR_TEST);
        float y = RenderSurface.Height - (scissorRect.Y + scissorRect.Height);
        GL.glScissor((int) scissorRect.X, (int) y, (int) scissorRect.Width, (int) scissorRect.Height);
    }

    public void SetRenderPipeline(IRenderPipeline shaderPipeline)
    {
        _renderPipeline = (OpenGLRenderPipeline) shaderPipeline;
        _renderPipeline.Use();
        _vertexStateDirty = true;
    }
    public void SetDepthStencilState(IDepthStencilState state)
    {
        if (state.DepthTestEnabled || state.DepthWriteEnabled) GL.glEnable(GL.GL_DEPTH_TEST);
        else GL.glDisable(GL.GL_DEPTH_TEST);

        GL.glDepthMask(state.DepthWriteEnabled ? GL.GL_TRUE : GL.GL_FALSE);

        GL.glDepthFunc(state.DepthTestEnabled
            ? GetDepthFunction(state.DepthCompareFunction)
            : GL.GL_ALWAYS);
    }

    public void SetVertexBuffer<T>(IBuffer<T> buffer, int slot = 0, uint offset = 0) where T : unmanaged
    {
        var b = (OpenGLBuffer<T>) buffer;
        _vbSlots[slot] = new VertexSlot { ID = b.Handle, Offset = offset };
        _vertexStateDirty = true;
    }
    
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

    public void SetTexture(ITexture texture, uint slot = 0)
    {
        Texture = (OpenGLTexture) texture;
        GL.glActiveTexture(GL.GL_TEXTURE0 + slot);
        GL.glBindTexture(GL.GL_TEXTURE_2D, Texture.Handle);
    }
    public void SetSampler(ISampler sampler, uint slot = 0)
    {
        Sampler = (OpenGLSampler) sampler;
        GL.glBindSampler(slot, Sampler.Handle);
    }

    void PreDraw()
    {
        if (Texture != null && Sampler != null && Sampler.MipmapFilter != MipmapFilter.None)
        {
            Texture.ValidateMipmaps();
        }
    }

    public void Draw(int startIndex, int vertexCount)
    {
        FlushVertexState();
        PreDraw();
        GL.glDrawArrays(GetPrimitiveType(_renderPipeline!.PrimitiveType), startIndex, vertexCount);
    }
    public void DrawIndexed(int startIndex, int indexCount)
    {
        FlushVertexState();
        _vertexState.BindIndexBuffer(_indexBufferHandle);
        PreDraw();
        GL.glDrawElements(GetPrimitiveType(_renderPipeline!.PrimitiveType), indexCount, _indexType,
                        startIndex * _indexSize);
    }
    public void DrawIndexed() => DrawIndexed(0, _indexCount);

    private void FlushVertexState()
    {
        if (!_vertexStateDirty) return;
        _vertexState.Apply(_renderPipeline!.GLLayout, _vbSlots);
        _vertexStateDirty = false;
    }

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