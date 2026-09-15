using System;

namespace odl3d.Renderer;

public interface IBuffer : IGPUResource
{
    nuint Size { get; }
    BufferUsage Usage { get; }
    BufferAccess Access { get; }

    void UpdateBuffer(ReadOnlySpan<byte> data);
}