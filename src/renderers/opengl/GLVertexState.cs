using System;

namespace odl3d.Renderer.OpenGLAdapter;

public struct VertexSlot { public uint ID; public uint Offset; }

public sealed class GLVertexState
{
    public uint Vao { get; }

    GLVertexLayout? _layout;
    readonly VertexSlot[] _slots = new VertexSlot[GLVertexLayout.MaxSlots];
    uint _enabledMask;
    uint _arrayBuffer;
    uint _elementBuffer;

    public GLVertexState()
    {
        GL.glGenVertexArrays(1, out uint vao);
        Vao = vao;
        GL.glBindVertexArray(Vao);            // stays bound for the device's lifetime
    }

    public void Apply(GLVertexLayout layout, VertexSlot[] pending)
    {
        bool layoutChanged = !ReferenceEquals(layout, _layout);

        foreach (ref readonly var a in layout.Attribs.AsSpan())
        {
            ref readonly var want = ref pending[a.Slot];
            ref readonly var have = ref _slots[a.Slot];   // compared against pre-update state
            if (!layoutChanged && have.Offset == want.Offset)
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

    public void BindIndexBuffer(uint id)
    {
        if (_elementBuffer == id) return;
        GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, id);
        _elementBuffer = id;
    }

    // Call right before glDeleteBuffers, on the GL thread.
    public void OnBufferDeleted(uint id)
    {
        for (int i = 0; i < _slots.Length; i++)
            if (_slots[i].ID == id) _slots[i] = default;
        if (_arrayBuffer == id) _arrayBuffer = 0;
        if (_elementBuffer == id) _elementBuffer = 0;
    }
}