namespace odl3d.Renderer;

/// <summary>
/// Defines the maximum number of vertex and uniform buffers, as well as the alignment requirement for uniform buffer offsets.
/// </summary>
public static class BufferSlots
{
    /// <summary>
    /// The maximum number of vertex buffers that can be bound simultaneously.
    /// The API's SetVertexBuffer slots 0..3 map to 27..30 in the shader.
    /// </summary>
    public const int MaxVertexBuffers  = 4;

    /// <summary>
    /// The maximum number of uniform buffers that can be bound simultaneously.
    /// The API's SetUniformBuffer slots 0..15 map to 0..15 in the shader.
    /// </summary>
    public const int MaxUniformBuffers = 16;

    /// <summary>
    /// The byte alignment requirement for uniform buffer offsets.
    /// </summary>
    public const int UniformOffsetAlignment = 256;
}