namespace odl3d.Renderer;

/// <summary>
/// Represents the different types of buffer usage in the renderer.
/// </summary>
public enum BufferUsage
{
    /// <summary>
    /// Represents a vertex buffer used for storing vertex data.
    /// </summary>
    Vertex,
    /// <summary>
    /// Represents an index buffer used for storing index data.
    /// </summary>
    Index,
    /// <summary>
    /// Represents a uniform buffer used for storing uniform data.
    /// </summary>
    Uniform,
    /// <summary>
    /// Represents a storage buffer used for storing arbitrary data.
    /// </summary>
    Storage,
    /// <summary>
    /// Represents an indirect buffer used for storing indirect draw commands.
    /// </summary>
    Indirect
}