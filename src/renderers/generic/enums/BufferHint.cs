namespace odl3d.Renderer;

/// <summary>
/// Represents hints for how a buffer will be used, which can help optimize performance.
/// </summary>
public enum BufferHint
{
    /// <summary>
    /// Indicates that the buffer will be set once and used many times.
    /// </summary>
    Static,
    /// <summary>
    /// Indicates that the buffer will be updated frequently and used many times.
    /// </summary>
    Dynamic,
    /// <summary>
    /// Indicates that the buffer will be updated frequently and used a few times.
    /// </summary>
    Stream
}