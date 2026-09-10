using System;

namespace odl3d;

/// <summary>
/// A GL vertex/index buffer pair for a textured quad (position xyz + uv per vertex).
/// </summary>
public class Mesh : IDisposable
{
    /// <summary>
    /// The GL vertex array object (VAO) handle for this mesh; used to bind the vertex/index buffers for drawing. The VAO encapsulates the vertex attribute configuration and buffer bindings.
    /// </summary>
    private uint _vao;

    /// <summary>
    /// The GL vertex buffer object (VBO) handle for this mesh; contains the vertex data (position xyz + uv per vertex). The VBO is bound to the VAO and used for drawing.
    /// </summary>
    private uint _vbo;

    /// <summary>
    /// The GL element buffer object (EBO) handle for this mesh; contains the index data for drawing the mesh. The EBO is bound to the VAO and used for indexed drawing.
    /// </summary>
    private uint _ebo;

    /// <summary>
    /// The number of indices in the mesh; used to determine how many elements to draw when rendering. This value is set during mesh creation and remains constant for the lifetime of the mesh.
    /// </summary>
    private readonly int _indexCount;

    /// <summary>
    /// The number of vertices stored in the mesh.
    /// </summary>
    public int VertexCount { get; }

    /// <summary>
    /// Indicates whether this mesh has been disposed and its resources released. After disposing, the mesh should not be used again.
    /// </summary>
    public bool Disposed { get; private set; } = false;

    /// <summary>
    /// Invoked when this mesh is disposed. Subscribers can use this event to perform cleanup or other actions when the mesh is no longer needed.
    /// </summary>
    public event Action? OnDisposed;

    /// <summary>
    /// Creates a new Mesh with the given vertex and index data. The vertex data should contain position (x, y, z) and texture coordinates (u, v) for each vertex, interleaved in the order [x, y, z, u, v]. The index data defines the triangles to draw using the vertices. This constructor generates the necessary GL buffers and configures the vertex attributes for rendering.
    /// </summary>
    /// <param name="vertices">The vertex data for the mesh, containing position (x, y, z) and texture coordinates (u, v) for each vertex, interleaved in the order [x, y, z, u, v].</param>
    /// <param name="indices">The index data defining the triangles to draw using the vertices.</param>
    public Mesh(float[] vertices, uint[] indices)
    {
        _indexCount = indices.Length;
        VertexCount = vertices.Length / 5;

        GL.glGenVertexArrays(1, out _vao);
        GL.glBindVertexArray(_vao);

        GL.glGenBuffers(1, out _vbo);
        GL.glBindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
        GL.glBufferDataFloat(GL.GL_ARRAY_BUFFER, (IntPtr)(vertices.Length * sizeof(float)), vertices, GL.GL_STATIC_DRAW);

        GL.glGenBuffers(1, out _ebo);
        GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
        GL.glBufferDataUInt(GL.GL_ELEMENT_ARRAY_BUFFER, (IntPtr)(indices.Length * sizeof(uint)), indices, GL.GL_STATIC_DRAW);

        int stride = 5 * sizeof(float);
        GL.glEnableVertexAttribArray(0);
        GL.glVertexAttribPointer(0, 3, GL.GL_FLOAT, 0, stride, IntPtr.Zero);
        GL.glEnableVertexAttribArray(1);
        GL.glVertexAttribPointer(1, 2, GL.GL_FLOAT, 0, stride, (IntPtr)(3 * sizeof(float)));

        GL.glBindVertexArray(0);
    }

    ~Mesh()
    {
        if (!Disposed) Console.WriteLine("Warning: Mesh was not disposed before being finalized. This may cause a GL resource leak.");
    }

    /// <summary>
    /// Returns a shared Mesh instance representing a unit quad centered at the origin, with vertices at (-0.5, -0.5), (0.5, -0.5), (0.5, 0.5), and (-0.5, 0.5), and texture coordinates (0, 1), (1, 1), (1, 0), and (0, 0). This quad can be used for rendering sprites or other textured objects in 2D or 3D space. The mesh is created once and reused for all calls to this property.
    /// </summary>
    public static Mesh Quad => _quad ??= CreateQuad();
    private static Mesh? _quad;

    /// <summary>
    /// Creates a new Mesh instance representing a unit quad centered at the origin, with texture coordinates scaled by the specified u and v multipliers. This allows for repeating or stretching the texture across the quad.
    /// </summary>
    /// <param name="uMult">The multiplier for the U (horizontal) texture coordinate.</param>
    /// <param name="vMult">The multiplier for the V (vertical) texture coordinate.</param>
    /// <returns>A new Mesh instance representing the unit quad with scaled texture coordinates.</returns>
    public static Mesh CreateQuad(float uMult = 1.0f, float vMult = 1.0f)
    {
        float[] vertices =
        {
            // x,     y,    z,      u,    v
            -0.5f, -0.5f, 0f,       0f, vMult,
             0.5f, -0.5f, 0f,       uMult, vMult,
             0.5f,  0.5f, 0f,       uMult, 0f,
            -0.5f,  0.5f, 0f,       0f, 0f,
        };
        uint[] indices = { 0, 1, 2, 2, 3, 0 };
        return new Mesh(vertices, indices);
    }

    /// <summary>
    /// Creates a new Mesh instance representing a plane with the given width, height, and length, centered at the origin. The plane's texture coordinates are scaled by the specified u and v multipliers, allowing for repeating or stretching the texture across the plane.
    /// </summary>
    /// <param name="width">The width of the plane.</param>
    /// <param name="height">The height of the plane.</param>
    /// <param name="length">The length of the plane along the z-axis.</param>
    /// <param name="uMult">The multiplier for the U (horizontal) texture coordinate.</param>
    /// <param name="vMult">The multiplier for the V (vertical) texture coordinate.</param>
    /// <returns>A new Mesh instance representing the plane with scaled texture coordinates.</returns>
    public static Mesh CreatePlane(float width, float height, float length, float uMult = 1.0f, float vMult = 1.0f)
    {
        float[] vertices =
        {
            // x,     y,    z,                  u,    v
            -width / 2f, 0f, -length / 2f,      0f, 0f, // bottom left (bottom)
             width / 2f, 0f, -length / 2f,      uMult, 0f, // bottom right (bottom)
             width / 2f, height, -length / 2f,  uMult, 0f, // bottom right (top)
            -width / 2f, height, -length / 2f,  0f, 0f, // bottom left (top)

            -width / 2f, 0f, length / 2f,       0f, vMult, // top left (bottom)
             width / 2f, 0f, length / 2f,       uMult, vMult, // top right (bottom)
             width / 2f, height, length / 2f,   uMult, vMult, // top right (top)
            -width / 2f, height, length / 2f,   0f, vMult, // top left (top)
        };
        uint[] indices = {
            0, 1, 2, 2, 3, 0, // front face
            4, 5, 6, 6, 7, 4, // back face
            0, 1, 5, 5, 4, 0, // bottom face
            3, 2, 6, 6, 7, 3, // top face
            1, 2, 6, 6, 5, 1, // right face
            0, 3, 7, 7, 4, 0 // left face
        };
        return new Mesh(vertices, indices);
    }

    /// <summary>
    /// Draws the mesh using the currently bound shader and texture. The mesh's vertex and index buffers are bound, and a draw call is issued to render the mesh as triangles. The mesh should be drawn after setting up the appropriate shader program and binding any required textures.
    /// </summary>
    public void Draw()
    {
        GL.glBindVertexArray(_vao);
        GL.glDrawElements(GL.GL_TRIANGLES, _indexCount, GL.GL_UNSIGNED_INT, IntPtr.Zero);
    }

    /// <summary>
    /// Disposes of the mesh, releasing its GL buffers and vertex array object. After calling this method, the mesh should not be used again. If the mesh has already been disposed, this method does nothing.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        GL.glDeleteBuffers(1, ref _ebo);
        GL.glDeleteBuffers(1, ref _vbo);
        GL.glDeleteVertexArrays(1, ref _vao);
        Disposed = true;
        OnDisposed?.Invoke();
    }

    /// <summary>
    /// Disposes of the shared quad mesh, releasing its GL buffers and vertex array object. After calling this method, the shared quad mesh should not be used again. This is useful for cleaning up resources when the application is shutting down. If the shared quad mesh has already been disposed, this method does nothing.
    /// </summary>
    public static void DisposeShared()
    {
        _quad?.Dispose();
        _quad = null;
    }
}
