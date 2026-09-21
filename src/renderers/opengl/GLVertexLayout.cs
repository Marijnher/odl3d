using System;

namespace odl3d.Renderer.OpenGLAdapter;

public readonly record struct GLAttrib(
    uint Location, int Components, uint Type,
    bool Normalized, bool IsInteger, int Offset, int Slot);

public sealed class GLVertexLayout
{
    public const int MaxSlots = 16;

    public readonly GLAttrib[] Attribs;
    public readonly int[] Strides = new int[MaxSlots];
    public readonly uint[] Divisors = new uint[MaxSlots];
    public readonly uint EnabledMask;

    public GLVertexLayout(VertexLayoutDescription d)
    {
        foreach (var b in d.Buffers)
        {
            Strides[b.BufferIndex] = (int)b.Stride;
            Divisors[b.BufferIndex] = b.StepFunction == StepMode.PerInstance ? 1u : 0u;
        }

        Attribs = new GLAttrib[d.Attributes.Length];
        for (int i = 0; i < Attribs.Length; i++)
        {
            var a = d.Attributes[i];
            var (comps, type, norm, isInt) = MapFormat(a.Format);
            Attribs[i] = new GLAttrib(a.AttributeIndex, comps, type, norm, isInt,
                                      a.Offset, (int)a.BufferSlot);
            EnabledMask |= 1u << (int)a.AttributeIndex;
        }
    }

    // Adapt to your VertexFormat enum
    static (int, uint, bool, bool) MapFormat(VertexFormat f) => f switch
    {
        VertexFormat.Float1 => (1, GL.GL_FLOAT, false, false),
        VertexFormat.Float2 => (2, GL.GL_FLOAT, false, false),
        VertexFormat.Float3 => (3, GL.GL_FLOAT, false, false),
        VertexFormat.Float4 => (4, GL.GL_FLOAT, false, false),
        VertexFormat.Half2  => (2, GL.GL_HALF_FLOAT, false, false),
        VertexFormat.Half4  => (4, GL.GL_HALF_FLOAT, false, false),
        VertexFormat.Int1   => (1, GL.GL_INT, false, true),
        VertexFormat.Int2   => (2, GL.GL_INT, false, true),
        VertexFormat.Int3   => (3, GL.GL_INT, false, true),
        VertexFormat.Int4   => (4, GL.GL_INT, false, true),
        VertexFormat.UInt1  => (1, GL.GL_UNSIGNED_INT, false, true),
        VertexFormat.UInt2  => (2, GL.GL_UNSIGNED_INT, false, true),
        VertexFormat.UInt3  => (3, GL.GL_UNSIGNED_INT, false, true),
        VertexFormat.UInt4  => (4, GL.GL_UNSIGNED_INT, false, true),
        _ => throw new NotSupportedException($"Vertex format {f}")
    };
}