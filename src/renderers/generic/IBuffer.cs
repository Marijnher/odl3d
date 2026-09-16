using System;

namespace odl3d.Renderer;

public interface IBuffer<T> : IGPUResource where T : unmanaged
{
    int Size { get; }
    BufferUsage Usage { get; }
    BufferAccess Access { get; }
    BufferType Type { get; }

    void SetData(T[] data);
}