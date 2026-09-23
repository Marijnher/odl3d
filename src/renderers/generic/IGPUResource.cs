using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents a generic GPU resource that can be disposed.
/// </summary>
public interface IGPUResource : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the GPU resource has been disposed.
    /// </summary>
    bool Disposed { get; }
}