using System;

namespace odl3d.Renderer.MetalAdapter;

internal static class MetalBufferSlots
{
    const uint MaxArgumentIndex = 31;   // Metal's per-stage buffer table has 31 entries

    public static uint VertexIndex(uint slot)
    {
        if (slot >= BufferSlots.MaxVertexBuffers)
            throw new ArgumentOutOfRangeException(nameof(slot),
                $"Vertex buffer slot must be 0..{BufferSlots.MaxVertexBuffers - 1}.");
        return MaxArgumentIndex - BufferSlots.MaxVertexBuffers + slot;   // 0..3 -> 27..30
    }
}