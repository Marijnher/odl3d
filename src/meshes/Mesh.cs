using System;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// A vertex/index buffer pair for a textured quad (position xyz + uv per vertex).
/// </summary>
public partial class Mesh : IDisposable
{
    /// <summary>
    /// Gets the renderer associated with this mesh, which is used to create and manage GPU resources.
    /// </summary>
    protected IRenderDevice Renderer => Window.Renderer;

    /// <summary>
    /// Gets the vertex buffer containing the mesh's vertex data, including positions, texture coordinates, and optionally normals.
    /// </summary>
    public IBuffer<float> Vertices { get; protected set; }

    /// <summary>
    /// Gets the index buffer containing the mesh's index data, which defines the triangles to draw using the vertices.
    /// </summary>
    public IBuffer<uint> Indices { get; protected set; }

    /// <summary>
    /// True if this mesh supplies a per-vertex normal in attribute 2, allowing a shader to light it. Meshes
    /// without normals leave that attribute disabled, so shaders that declare it still work unchanged.
    /// </summary>
    public bool HasNormals { get; }

    /// <summary>
    /// Gets the number of vertices in this mesh.
    /// </summary>
    public int VertexCount { get; private set; }

    /// <summary>
    /// Indicates whether this mesh has been disposed and its resources released. After disposing, the mesh should not be used again.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Invoked when this mesh is disposed. Subscribers can use this event to perform cleanup or other actions when the mesh is no longer needed.
    /// </summary>
    public event Action? OnDisposed;

    /// <summary>
    /// Creates a new Mesh with the given vertex and index data. The vertex data should contain position (x, y, z) and texture coordinates (u, v) for each vertex, interleaved in the order [x, y, z, u, v]. The index data defines the triangles to draw using the vertices. This constructor generates the necessary renderer buffers and configures the vertex attributes for rendering.
    /// </summary>
    /// <param name="vertices">The vertex data for the mesh, containing position (x, y, z) and texture coordinates (u, v) for each vertex, interleaved in the order [x, y, z, u, v].</param>
    /// <param name="indices">The index data defining the triangles to draw using the vertices.</param>
    public Mesh(float[] vertices, uint[] indices) : this(vertices, indices, false) { }

    /// <summary>
    /// Creates a new Mesh with the given vertex and index data, optionally including a per-vertex normal. With normals, each vertex is interleaved in the order [x, y, z, u, v, nx, ny, nz] and the normal is bound to vertex attribute 2; without them the layout is [x, y, z, u, v] as usual.
    /// </summary>
    /// <param name="vertices">The interleaved vertex data for the mesh.</param>
    /// <param name="indices">The index data defining the triangles to draw using the vertices.</param>
    /// <param name="hasNormals">True if the vertex data includes a normal after the texture coordinates.</param>
    public Mesh(float[] vertices, uint[] indices, bool hasNormals)
    {
        Vertices = Renderer.CreateBuffer(new BufferDescription
        {
            Size = vertices.Length,
            Usage = BufferUsage.Vertex
        }, vertices);
        Indices = Renderer.CreateBuffer(new BufferDescription
        {
            Size = indices.Length,
            Usage = BufferUsage.Index
        }, indices);

        HasNormals = hasNormals;
        int floatsPerVertex = hasNormals ? 8 : 5;
        VertexCount = vertices.Length / floatsPerVertex;
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
        Vertices.Dispose();
        Indices.Dispose();
        Disposed = true;
        OnDisposed?.Invoke();
    }
}
