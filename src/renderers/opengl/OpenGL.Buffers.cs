using System;
using odl3d;
using static odl3d.GL;

namespace odl3d.Renderers;

public partial class OpenGL : IRenderer
{
    public uint CreateVertexArray()
    {
        glGenVertexArrays(1, out uint vao);
        return vao;
    }

    public void DeleteVertexArray(uint vao) => glDeleteVertexArrays(1, ref vao);

    public void BindVertexArray(uint vao) => glBindVertexArray(vao);

    public uint CreateBuffer()
    {
        glGenBuffers(1, out uint buffer);
        return buffer;
    }

    public void DeleteBuffer(uint buffer) => glDeleteBuffers(1, ref buffer);

    private uint GetGLBufferTarget(BufferTarget target) => target switch
    {
        BufferTarget.ArrayBuffer => GL_ARRAY_BUFFER,
        BufferTarget.ElementBuffer => GL_ELEMENT_ARRAY_BUFFER,
        _ => throw new ArgumentOutOfRangeException(nameof(target), $"Unsupported buffer target: {target}")
    };

    private uint GetGLBufferHint(BufferHint hint) => hint switch
    {
        BufferHint.Static => GL_STATIC_DRAW,
        BufferHint.Dynamic => GL_DYNAMIC_DRAW,
        BufferHint.Stream => GL_STREAM_DRAW,
        _ => throw new ArgumentOutOfRangeException(nameof(hint), $"Unsupported buffer hint: {hint}")
    };

    public void BindBuffer(BufferTarget target, uint buffer) => glBindBuffer(GetGLBufferTarget(target), buffer);

    public void SetBufferData(BufferTarget target, uint buffer, float[] data, BufferHint hint) =>
        glBufferDataFloat(GetGLBufferTarget(target), data.Length * sizeof(float), data, GetGLBufferHint(hint));

    public void SetBufferData(BufferTarget target, uint buffer, uint[] data, BufferHint hint) =>
        glBufferDataUInt(GetGLBufferTarget(target), data.Length * sizeof(uint), data, GetGLBufferHint(hint));

    public void EnableVertexAttribute(int index) => glEnableVertexAttribArray((uint) index);

    public void AddVertexAttribute(int index, int size, int stride, int offset) =>
        glVertexAttribPointer((uint) index, size, GL_FLOAT, 0, stride, (nint) offset * sizeof(float));
}