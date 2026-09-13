using System;

namespace odl3d;

/// <summary>
/// Specifies the expected usage pattern of a buffer object's data store.
/// </summary>
public enum BufferHint
{
    /// <summary>
    /// The data store contents will be modified once and used many times.
    /// </summary>
    Static,

    /// <summary>
    /// The data store contents will be modified repeatedly and used many times.
    /// </summary>
    Dynamic,

    /// <summary>
    /// The data store contents will be modified once and used at most a few times.
    /// </summary>
    Stream
}