using System;

namespace odl3d;

/// <summary>
/// Specifies the target buffer object for renderer buffer operations.
/// </summary>
public enum BufferTarget
{
    /// <summary>
    /// Specifies that the buffer object is an array buffer, which is used to store vertex attributes.
    /// </summary>
    ArrayBuffer,

    /// <summary>
    /// Specifies that the buffer object is an element array buffer, which is used to store indices for indexed drawing.
    /// </summary>
    ElementBuffer
}