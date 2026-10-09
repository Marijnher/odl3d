using System;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// A vertex/index buffer pair containing position, texture coordinates, and normal data per vertex.
/// </summary>
public partial class Mesh : IDisposable
{
    private readonly Vertex[] _vertexData;
    private readonly uint[] _indexData;
    private IRenderDevice? _device;
    private IBuffer<Vertex>? _vertices;
    private IBuffer<uint>? _indices;

    /// <summary>
    /// Gets the vertex buffer containing the mesh's vertex data, including positions, texture coordinates, and optionally normals.
    /// </summary>
    public IBuffer<Vertex> Vertices => _vertices ?? throw new InvalidOperationException("The mesh has not been uploaded to a renderer.");

    /// <summary>
    /// Gets the index buffer containing the mesh's index data, which defines the triangles to draw using the vertices.
    /// </summary>
    public IBuffer<uint> Indices => _indices ?? throw new InvalidOperationException("The mesh has not been uploaded to a renderer.");

    /// <summary>
    /// True when every vertex has a usable normal. This is always the case: normals missing from the source are generated as flat normals, see <see cref="HasGeneratedNormals"/>.
    /// </summary>
    public bool HasNormals => true;

    /// <summary>
    /// True when flat normals were generated because the source geometry lacked normals. Generating them separates vertices that are shared between faces with different normals, so <see cref="VertexCount"/> can be higher than the vertex count passed in.
    /// </summary>
    public bool HasGeneratedNormals { get; }

    /// <summary>
    /// Gets the number of vertices in this mesh.
    /// </summary>
    public int VertexCount { get; private set; }

    /// <summary>
    /// Gets the axis-aligned bounds of the mesh in its local coordinate space.
    /// </summary>
    public BoundingBox3D Bounds { get; }

    /// <summary>
    /// Indicates whether this mesh has been disposed and its resources released. After disposing, the mesh should not be used again.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Invoked when this mesh is disposed. Subscribers can use this event to perform cleanup or other actions when the mesh is no longer needed.
    /// </summary>
    public event Action? OnDisposed;

    /// <summary>
    /// Creates a new Mesh with the given vertex and index data. Each vertex contains a position, texture coordinate, and normal; flat normals are generated when the source has none.
    /// </summary>
    /// <param name="vertices">The vertex data for the mesh.</param>
    /// <param name="indices">The index data defining the triangles to draw using the vertices.</param>
    /// <param name="hasNormals">True when the vertices contain meaningful source normals; otherwise (or for vertices with a zero normal) flat normals are generated per triangle.</param>
    public Mesh(Vertex[] vertices, uint[] indices, bool hasNormals = false)
    {
        // Bounds come from the source vertices, so they do not change when normal generation drops unreferenced ones.
        BoundingBox3D bounds = BoundingBox3D.Empty;
        foreach (Vertex vertex in vertices)
            bounds = bounds.Include(vertex.Position);
        Bounds = bounds;

        (vertices, indices, bool generated) = FlatNormals.Apply(vertices, indices, hasNormals);
        _vertexData = vertices;
        _indexData = indices;
        HasGeneratedNormals = generated;
        VertexCount = vertices.Length;
    }

    internal void EnsureUploaded(IRenderDevice device)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        if (_device != null && !ReferenceEquals(_device, device))
            throw new InvalidOperationException("A mesh can only be used with the renderer that first uploaded it.");
        if (_vertices != null) return;

        _device = device;
        _vertices = device.CreateBuffer(new BufferDescription
        {
            Size = _vertexData.Length,
            Usage = BufferUsage.Vertex
        }, _vertexData);
        _indices = device.CreateBuffer(new BufferDescription
        {
            Size = _indexData.Length,
            Usage = BufferUsage.Index
        }, _indexData);
    }

    ~Mesh()
    {
        if (!Disposed) Console.WriteLine("Warning: Mesh was not disposed before being finalized. This may cause a renderer resource leak.");
    }

    /// <summary>
    /// Disposes of the mesh, releasing its renderer buffers and vertex array object. After calling this method, the mesh should not be used again. If the mesh has already been disposed, this method does nothing.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        _vertices?.Dispose();
        _indices?.Dispose();
        Disposed = true;
        OnDisposed?.Invoke();
    }
}
