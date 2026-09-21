using System;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLBuffer<T> : IBuffer<T> where T : unmanaged
{
    public uint Handle { get; }

    public int Size { get; }
    public BufferUsage Usage { get; }
    public BufferHint Hint { get; }

    public bool Disposed { get; private set; }

    public OpenGLBuffer(BufferDescription description, T[]? initialData = null)
    {
        Size = description.Size;
        Usage = description.Usage;
        Hint = description.Hint;
        GL.glGenBuffers(1, out uint handle);
        Handle = handle;
        if (initialData != null) SetData(initialData);
    }

    private uint GetHintType() => Hint switch
    {
        BufferHint.Static => GL.GL_STATIC_DRAW,
        BufferHint.Dynamic => GL.GL_DYNAMIC_DRAW,
        BufferHint.Stream => GL.GL_STREAM_DRAW,
        _ => throw new RenderException("Unsupported buffer hint.")
    };

    public unsafe void SetData(T[] data)
    {
        SetData(data, 0, data.Length);
    }

    public unsafe void SetData(T[] data, int offset, int count)
    {
        GL.glBindBuffer(GL.GL_COPY_WRITE_BUFFER, Handle);
        fixed (T* dataPtr = data) GL.glBufferData(GL.GL_COPY_WRITE_BUFFER, count * sizeof(T), (nint) (dataPtr + offset), GetHintType());
    }

    public void Dispose()
    {
        if (Disposed) return;
        uint handle = Handle;
        GL.glDeleteBuffers(1, ref handle);
        Disposed = true;
    }
}