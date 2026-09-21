using System;

namespace odl3d.Renderer;

public interface IBuffer<T> : IGPUResource where T : unmanaged
{
    int Size { get; }
    BufferUsage Usage { get; }
    BufferHint Hint { get; }

    void SetData(T[] data);
    void SetData(T[] data, int offset, int count);
}