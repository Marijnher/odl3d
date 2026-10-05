using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents a vertex buffer slot, including its ID and offset within the buffer.
/// </summary>
internal readonly record struct VertexSlot(uint ID, uint Offset);

/// <summary>
/// Represents the state of the OpenGL vertex array, including the currently bound vertex layout, vertex buffer slots, and index buffer.
/// </summary>
internal sealed class GLVertexState : IDisposable
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
        GL.glBindVertexArray(Vao);
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

    public void Dispose()
    {
        uint vao = Vao;
        GL.glDeleteVertexArrays(1, ref vao);
    }
}

/// <summary>
/// Reuses vertex array objects for matching layouts and buffer bindings.
/// </summary>
internal sealed class GLVertexArrayCache
{
    private const int MaxEntries = 256;

    private sealed class Entry
    {
        public required int Hash { get; init; }
        public required GLVertexLayout Layout { get; init; }
        public required VertexSlot[] Slots { get; init; }
        public required uint IndexBuffer { get; init; }
        public required GLVertexState State { get; init; }
    }

    private readonly Dictionary<int, List<Entry>> _entries = new();
    private readonly Queue<Entry> _insertionOrder = new();
    private int _count;

    public void Bind(GLVertexLayout layout, VertexSlot[] slots, uint indexBuffer)
    {
        int hash = GetHash(layout, slots, indexBuffer);
        if (_entries.TryGetValue(hash, out List<Entry>? bucket))
        {
            foreach (Entry entry in bucket)
            {
                if (!ReferenceEquals(entry.Layout, layout) || entry.IndexBuffer != indexBuffer || !entry.Slots.AsSpan().SequenceEqual(slots))
                    continue;

                GL.glBindVertexArray(entry.State.Vao);
                return;
            }
        }

        GLVertexState state = new();
        state.Apply(layout, slots);
        state.BindIndexBuffer(indexBuffer);
        Entry added = new()
        {
            Hash = hash,
            Layout = layout,
            Slots = (VertexSlot[]) slots.Clone(),
            IndexBuffer = indexBuffer,
            State = state
        };
        if (!_entries.TryGetValue(hash, out bucket))
            _entries.Add(hash, bucket = new List<Entry>());
        bucket.Add(added);
        _insertionOrder.Enqueue(added);
        _count++;

        while (_count > MaxEntries)
            Remove(_insertionOrder.Dequeue());
    }

    public void OnBufferDeleted(uint id)
    {
        List<Entry> staleEntries = new();
        foreach (List<Entry> bucket in _entries.Values)
        {
            foreach (Entry entry in bucket)
            {
                bool containsBuffer = entry.IndexBuffer == id;
                for (int i = 0; !containsBuffer && i < entry.Slots.Length; i++)
                    containsBuffer = entry.Slots[i].ID == id;
                if (containsBuffer) staleEntries.Add(entry);
            }
        }
        foreach (Entry entry in staleEntries)
            Remove(entry);
    }

    public void Clear()
    {
        foreach (List<Entry> bucket in _entries.Values)
            foreach (Entry entry in bucket)
                entry.State.Dispose();
        _entries.Clear();
        _insertionOrder.Clear();
        _count = 0;
    }

    private static int GetHash(GLVertexLayout layout, VertexSlot[] slots, uint indexBuffer)
    {
        HashCode hash = new();
        hash.Add(RuntimeHelpers.GetHashCode(layout));
        hash.Add(indexBuffer);
        foreach (VertexSlot slot in slots)
            hash.Add(slot);
        return hash.ToHashCode();
    }

    private void Remove(Entry entry)
    {
        if (!_entries.TryGetValue(entry.Hash, out List<Entry>? bucket) || !bucket.Remove(entry)) return;
        if (bucket.Count == 0) _entries.Remove(entry.Hash);
        entry.State.Dispose();
        _count--;
    }
}