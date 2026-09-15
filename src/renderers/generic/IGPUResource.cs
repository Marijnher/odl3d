using System;

namespace odl3d.Renderer;

public interface IGPUResource : IDisposable
{
    bool Disposed { get; }
}