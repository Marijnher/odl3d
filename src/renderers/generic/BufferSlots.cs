namespace odl3d.Renderer;

public static class BufferSlots
{
    public const int MaxVertexBuffers  = 4;         // API: SetVertexBuffer slots 0..3
    public const int MaxUniformBuffers = 16;        // API: SetUniformBuffer slots 0..15
    public const int UniformOffsetAlignment = 256;  // Alignment requirement for uniform buffer offsets
}