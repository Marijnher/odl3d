using System;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Provides utility methods for calculating Metal buffer argument indices based on buffer slots.
/// </summary>
internal static class MetalBufferSlots
{
    /// <summary>
    /// The maximum argument index for Metal buffer slots.
    /// </summary>
    const uint MaxArgumentIndex = 31;   // Metal's per-stage buffer table has 31 entries

    /// <summary>
    /// Calculates the Metal buffer argument index for a given vertex buffer slot.
    /// </summary>
    /// <param name="slot">The vertex buffer slot.</param>
    /// <returns>The corresponding Metal buffer argument index.</returns>
    public static uint VertexIndex(uint slot)
    {
        if (slot >= BufferSlots.MaxVertexBuffers)
            throw new ArgumentOutOfRangeException(nameof(slot),
                $"Vertex buffer slot must be 0..{BufferSlots.MaxVertexBuffers - 1}.");
        return MaxArgumentIndex - BufferSlots.MaxVertexBuffers + slot;   // 0..3 -> 27..30
    }
}