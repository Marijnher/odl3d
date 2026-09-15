using System;

namespace odl3d;

/// <summary>
/// A vertex/index buffer pair for a textured quad (position xyz + uv per vertex).
/// </summary>
public partial class Mesh : IDisposable
{
    /// <summary>
    /// The renderer instance used to create and manage this mesh. The Renderer property provides access to the active renderer, allowing the Mesh to call renderer methods for creating buffers, binding vertex arrays, drawing elements, and managing resources. This property is used internally by the Mesh class to interact with the rendering backend.
    /// </summary>
    protected IRendererOld Renderer => RenderFactory.Renderer;

    /// <summary>
    /// The renderer-backed vertex array object (VAO) handle for this mesh; used to bind the vertex/index buffers for drawing. The VAO encapsulates the vertex attribute configuration and buffer bindings.
    /// </summary>
    private VertexArray vao;

    /// <summary>
    /// The renderer-backed vertex buffer object (VBO) handle for this mesh; contains the vertex data (position xyz + uv per vertex). The VBO is bound to the VAO and used for drawing.
    /// </summary>
    private Buffer vbo;

    /// <summary>
    /// The renderer-backed element buffer object (EBO) handle for this mesh; contains the index data for drawing the mesh. The EBO is bound to the VAO and used for indexed drawing.
    /// </summary>
    private Buffer ebo;

    /// <summary>
    /// The number of indices in the mesh; used to determine how many elements to draw when rendering. This value is set during mesh creation and remains constant for the lifetime of the mesh.
    /// </summary>
    private readonly int _indexCount;

    /// <summary>
    /// The number of vertices stored in the mesh.
    /// </summary>
    public int VertexCount { get; }

    /// <summary>
    /// True if this mesh supplies a per-vertex normal in attribute 2, allowing a shader to light it. Meshes
    /// without normals leave that attribute disabled, so shaders that declare it still work unchanged.
    /// </summary>
    public bool HasNormals { get; }

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
        HasNormals = hasNormals;
        int floatsPerVertex = hasNormals ? 8 : 5;
        _indexCount = indices.Length;
        VertexCount = vertices.Length / floatsPerVertex;

        vao = new VertexArray();
        vao.Bind();

        vbo = new Buffer(BufferTarget.ArrayBuffer);
        vbo.SetData(vertices);

        ebo = new Buffer(BufferTarget.ElementBuffer);
        ebo.SetData(indices);

        vao.AddAttribute(3); // location=0 x y z
        vao.AddAttribute(2); // location=1 u v
        if (hasNormals) vao.AddAttribute(3); // location=2 nx ny nz

        vao.Unbind(); 
    }

    ~Mesh()
    {
        if (!Disposed) Console.WriteLine("Warning: Mesh was not disposed before being finalized. This may cause a renderer resource leak.");
    }

    /// <summary>
    /// Draws the mesh using the currently bound shader and texture. The mesh's vertex and index buffers are bound, and a draw call is issued to render the mesh as triangles. The mesh should be drawn after setting up the appropriate shader program and binding any required textures.
    /// </summary>
    public void Draw()
    {
        vao.Bind();
        Renderer.DrawElements(_indexCount);
    }

    /// <summary>
    /// Disposes of the mesh, releasing its renderer buffers and vertex array object. After calling this method, the mesh should not be used again. If the mesh has already been disposed, this method does nothing.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        ebo.Dispose();
        vbo.Dispose();
        vao.Dispose();
        Disposed = true;
        OnDisposed?.Invoke();
    }
}
