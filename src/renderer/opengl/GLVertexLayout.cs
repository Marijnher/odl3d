using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL vertex attribute description.
/// </summary>
/// <param name="Location">The location of the vertex attribute.</param>
/// <param name="Components">The number of components in the vertex attribute.</param>
/// <param name="Type">The data type of the vertex attribute.</param>
/// <param name="Normalized">Indicates whether the vertex attribute is normalized.</param>
/// <param name="IsInteger">Indicates whether the vertex attribute is an integer type.</param>
/// <param name="Offset">The offset of the vertex attribute within the vertex buffer.</param>
/// <param name="Slot">The vertex buffer slot to which the attribute belongs.</param>
internal readonly record struct GLAttrib(
    uint Location, int Components, uint Type,
    bool Normalized, bool IsInteger, int Offset, int Slot);

/// <summary>
/// Represents an OpenGL vertex layout, including attribute descriptions, buffer strides, and divisors.
/// </summary>
internal sealed class GLVertexLayout
{
    /// <summary>
    /// The maximum number of vertex buffer slots supported.
    /// </summary>
    public const int MaxSlots = 16;

    /// <summary>
    /// The array of vertex attribute descriptions.
    /// </summary>
    public readonly GLAttrib[] Attribs;

    /// <summary>
    /// The array of vertex buffer strides.
    /// </summary>
    public readonly int[] Strides = new int[MaxSlots];

    /// <summary>
    /// The array of vertex buffer divisors.
    /// </summary>
    public readonly uint[] Divisors = new uint[MaxSlots];

    /// <summary>
    /// The bitmask indicating which vertex attributes are enabled.
    /// </summary>
    public readonly uint EnabledMask;

    /// <summary>
    /// Initializes a new instance of the <see cref="GLVertexLayout"/> class based on the specified vertex layout description.
    /// </summary>
    /// <param name="d">The vertex layout description used to initialize the layout.</param>
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

    /// <summary>
    /// Maps a vertex format to its corresponding OpenGL attribute description.
    /// </summary>
    /// <param name="f">The vertex format to map to an OpenGL attribute description.</param>
    /// <returns>A tuple containing the number of components, the OpenGL type, whether the attribute is normalized, and whether it is an integer attribute.</returns>
    /// <exception cref="RenderException">Thrown when the vertex format is not supported.</exception>
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
        _ => throw new RenderException($"Vertex format {f}")
    };
}