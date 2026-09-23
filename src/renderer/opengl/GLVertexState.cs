using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents a vertex buffer slot, including its ID and offset within the buffer.
/// </summary>
internal struct VertexSlot { public uint ID; public uint Offset; }

/// <summary>
/// Represents the state of the OpenGL vertex array, including the currently bound vertex layout, vertex buffer slots, and index buffer.
/// </summary>
internal sealed class GLVertexState
{
    /// <summary>
    /// Gets the ID of the OpenGL vertex array object (VAO).
    /// </summary>
    public uint Vao { get; }

    GLVertexLayout? _layout;
    readonly VertexSlot[] _slots = new VertexSlot[GLVertexLayout.MaxSlots];
    uint _enabledMask;
    uint _arrayBuffer;
    uint _elementBuffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="GLVertexState"/> class and creates a new OpenGL vertex array object (VAO).
    /// </summary>
    public GLVertexState()
    {
        GL.glGenVertexArrays(1, out uint vao);
        Vao = vao;
        GL.glBindVertexArray(Vao);            // stays bound for the device's lifetime
    }

    /// <summary>
    /// Applies the specified vertex layout and pending vertex buffer slots to the OpenGL vertex array state.
    /// </summary>
    /// <param name="layout">The vertex layout to apply.</param>
    /// <param name="pending">The array of pending vertex buffer slots to apply.</param>
    public void Apply(GLVertexLayout layout, VertexSlot[] pending)
    {
        bool layoutChanged = !ReferenceEquals(layout, _layout);

        foreach (ref readonly var a in layout.Attribs.AsSpan())
        {
            ref readonly var want = ref pending[a.Slot];
            ref readonly var have = ref _slots[a.Slot];   // compared against pre-update state
            if (!layoutChanged && have.ID == want.ID && have.Offset == want.Offset)
                continue;

            if (_arrayBuffer != want.ID)
            {
                GL.glBindBuffer(GL.GL_ARRAY_BUFFER, want.ID);
                _arrayBuffer = want.ID;
            }

            nint ptr = (nint)(want.Offset + (uint)a.Offset);
            int stride = layout.Strides[a.Slot];
            if (a.IsInteger)
                GL.glVertexAttribIPointer(a.Location, a.Components, a.Type, stride, ptr);
            else
                GL.glVertexAttribPointer(a.Location, a.Components, a.Type, (byte) (a.Normalized ? 1 : 0), stride, ptr);
            GL.glVertexAttribDivisor(a.Location, layout.Divisors[a.Slot]);
        }

        // Record applied slots only after the loop, so several attributes
        // sharing one slot all get re-specified.
        foreach (ref readonly var a in layout.Attribs.AsSpan())
            _slots[a.Slot] = pending[a.Slot];

        uint changed = layout.EnabledMask ^ _enabledMask;
        for (int i = 0; changed != 0; i++, changed >>= 1)
        {
            if ((changed & 1) == 0) continue;
            if ((layout.EnabledMask & (1u << i)) != 0) GL.glEnableVertexAttribArray(i);
            else GL.glDisableVertexAttribArray(i);
        }
        _enabledMask = layout.EnabledMask;
        _layout = layout;
    }

    /// <summary>
    /// Binds the specified index buffer to the OpenGL vertex array state.
    /// </summary>
    /// <param name="id">The ID of the index buffer to bind.</param>
    public void BindIndexBuffer(uint id)
    {
        if (_elementBuffer == id) return;
        GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, id);
        _elementBuffer = id;
    }

    /// <summary>
    /// Handles the deletion of a buffer by updating the OpenGL vertex array state accordingly.
    /// </summary>
    /// <param name="id">The ID of the buffer that was deleted.</param>
    public void OnBufferDeleted(uint id)
    {
        for (int i = 0; i < _slots.Length; i++)
            if (_slots[i].ID == id) _slots[i] = default;
        if (_arrayBuffer == id) _arrayBuffer = 0;
        if (_elementBuffer == id) _elementBuffer = 0;
    }
}