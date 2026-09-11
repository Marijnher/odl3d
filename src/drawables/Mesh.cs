using System;
using System.Collections.Generic;

namespace odl3d;

/// <summary>
/// A GL vertex/index buffer pair for a textured quad (position xyz + uv per vertex).
/// </summary>
public class Mesh : IDisposable
{
    private readonly record struct Vertex(float X, float Y, float Z, float U, float V);

    private readonly record struct PlaneSideUvs(float LeftU, float RightU, float TopV, float BottomV);

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
    /// True if this mesh supplies a per-vertex normal in attribute 2, allowing a shader to light it. Meshes
    /// without normals leave that attribute disabled, so shaders that declare it still work unchanged.
    /// </summary>
    public bool HasNormals { get; }

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

        GL.glGenVertexArrays(1, out _vao);
        GL.glBindVertexArray(_vao);

        GL.glGenBuffers(1, out _vbo);
        GL.glBindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
        GL.glBufferDataFloat(GL.GL_ARRAY_BUFFER, (IntPtr)(vertices.Length * sizeof(float)), vertices, GL.GL_STATIC_DRAW);

        GL.glGenBuffers(1, out _ebo);
        GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
        GL.glBufferDataUInt(GL.GL_ELEMENT_ARRAY_BUFFER, (IntPtr)(indices.Length * sizeof(uint)), indices, GL.GL_STATIC_DRAW);

        int stride = floatsPerVertex * sizeof(float);
        GL.glEnableVertexAttribArray(0);
        GL.glVertexAttribPointer(0, 3, GL.GL_FLOAT, 0, stride, IntPtr.Zero);
        GL.glEnableVertexAttribArray(1);
        GL.glVertexAttribPointer(1, 2, GL.GL_FLOAT, 0, stride, (IntPtr)(3 * sizeof(float)));
        if (hasNormals)
        {
            GL.glEnableVertexAttribArray(2);
            GL.glVertexAttribPointer(2, 3, GL.GL_FLOAT, 0, stride, (IntPtr)(5 * sizeof(float)));
        }

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
    /// Returns a shared Mesh instance identical to Quad but with its V texture coordinate flipped, so a
    /// top-down CPU pixel buffer (row 0 = top, as produced by Texture) samples right-side up once rendered
    /// through Scene2D's ortho projection, without needing to flip the pixel data itself.
    /// </summary>
    public static Mesh QuadFlippedV => _quadFlippedV ??= CreateQuad(flipV: true);
    private static Mesh? _quadFlippedV;

    /// <summary>
    /// Creates a new Mesh instance representing a unit quad centered at the origin, with texture coordinates scaled by the specified u and v multipliers. This allows for repeating or stretching the texture across the quad.
    /// </summary>
    /// <param name="uMult">The multiplier for the U (horizontal) texture coordinate.</param>
    /// <param name="vMult">The multiplier for the V (vertical) texture coordinate.</param>
    /// <param name="flipV">If true, swaps the top/bottom V coordinates so the texture samples upright rather than upside-down.</param>
    /// <returns>A new Mesh instance representing the unit quad with scaled texture coordinates.</returns>
    public static Mesh CreateQuad(float uMult = 1.0f, float vMult = 1.0f, bool flipV = false)
    {
        float vTop = flipV ? vMult : 0f;
        float vBottom = flipV ? 0f : vMult;
        float[] vertices =
        {
            // x,     y,    z,      u,    v
            -0.5f, -0.5f, 0f,       0f, vBottom,
             0.5f, -0.5f, 0f,       uMult, vBottom,
             0.5f,  0.5f, 0f,       uMult, vTop,
            -0.5f,  0.5f, 0f,       0f, vTop,
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
    /// <param name="flipV">If true, swaps the top/bottom V coordinates so the texture samples upright rather than upside-down.</param>
    /// <param name="sampleEdgesOnSides">If true, side faces sample only the nearest edge row or column of the texture.</param>
    /// <returns>A new Mesh instance representing the plane with scaled texture coordinates.</returns>
    public static Mesh CreatePlane(float width, float height, float length, float uMult = 1.0f, float vMult = 1.0f, bool flipV = false, bool sampleEdgesOnSides = false)
    {
        float vTop = flipV ? vMult : 0f;
        float vBottom = flipV ? 0f : vMult;
        PlaneSideUvs? sideUvs = sampleEdgesOnSides ? new PlaneSideUvs(0f, uMult, vTop, vBottom) : null;
        return CreatePlaneMesh(width, height, length, 0f, uMult, vTop, vBottom, sideUvs, false);
    }

    public static Mesh CreateTextPlane(float width, float height, float length, float leftU, float rightU, float topV, float bottomV) =>
        CreatePlaneMesh(width, height, length, 0f, 1f, 0f, 1f, new PlaneSideUvs(leftU, rightU, topV, bottomV), true);

    private static Mesh CreatePlaneMesh(float width, float height, float length, float faceLeftU, float faceRightU, float faceTopV, float faceBottomV, PlaneSideUvs? sideUvs, bool backMatchesFront)
    {
        List<float> vertices = new();
        List<uint> indices = new();

        AddFrontFace(vertices, indices, width, height, length, faceLeftU, faceRightU, faceTopV, faceBottomV);
        AddBackFace(vertices, indices, width, height, length, faceLeftU, faceRightU, faceTopV, faceBottomV, backMatchesFront);
        AddBottomFace(vertices, indices, width, length, faceLeftU, faceRightU, faceTopV, faceBottomV, sideUvs);
        AddTopFace(vertices, indices, width, height, length, faceLeftU, faceRightU, faceTopV, faceBottomV, sideUvs);
        AddRightFace(vertices, indices, width, height, length, faceLeftU, faceRightU, faceTopV, faceBottomV, sideUvs);
        AddLeftFace(vertices, indices, width, height, length, faceLeftU, faceRightU, faceTopV, faceBottomV, sideUvs);

        return new Mesh(vertices.ToArray(), indices.ToArray());
    }

    private static void AddFrontFace(List<float> vertices, List<uint> indices, float width, float height, float length, float leftU, float rightU, float topV, float bottomV) =>
        AddFace(vertices, indices,
            new Vertex(-width / 2f, 0f, length / 2f, leftU, bottomV),
            new Vertex(width / 2f, 0f, length / 2f, rightU, bottomV),
            new Vertex(width / 2f, height, length / 2f, rightU, topV),
            new Vertex(-width / 2f, height, length / 2f, leftU, topV));

    private static void AddBackFace(List<float> vertices, List<uint> indices, float width, float height, float length, float leftU, float rightU, float topV, float bottomV, bool matchesFront) =>
        AddFace(vertices, indices,
            new Vertex(width / 2f, 0f, -length / 2f, matchesFront ? rightU : leftU, bottomV),
            new Vertex(-width / 2f, 0f, -length / 2f, matchesFront ? leftU : rightU, bottomV),
            new Vertex(-width / 2f, height, -length / 2f, matchesFront ? leftU : rightU, topV),
            new Vertex(width / 2f, height, -length / 2f, matchesFront ? rightU : leftU, topV));

    private static void AddBottomFace(List<float> vertices, List<uint> indices, float width, float length, float leftU, float rightU, float topV, float bottomV, PlaneSideUvs? sideUvs) =>
        AddFace(vertices, indices,
            new Vertex(-width / 2f, 0f, -length / 2f, leftU, bottomV),
            new Vertex(width / 2f, 0f, -length / 2f, rightU, bottomV),
            new Vertex(width / 2f, 0f, length / 2f, rightU, sideUvs?.BottomV ?? topV),
            new Vertex(-width / 2f, 0f, length / 2f, leftU, sideUvs?.BottomV ?? topV));

    private static void AddTopFace(List<float> vertices, List<uint> indices, float width, float height, float length, float leftU, float rightU, float topV, float bottomV, PlaneSideUvs? sideUvs) =>
        AddFace(vertices, indices,
            new Vertex(-width / 2f, height, length / 2f, leftU, topV),
            new Vertex(width / 2f, height, length / 2f, rightU, topV),
            new Vertex(width / 2f, height, -length / 2f, rightU, sideUvs?.TopV ?? bottomV),
            new Vertex(-width / 2f, height, -length / 2f, leftU, sideUvs?.TopV ?? bottomV));

    private static void AddRightFace(List<float> vertices, List<uint> indices, float width, float height, float length, float leftU, float rightU, float topV, float bottomV, PlaneSideUvs? sideUvs) =>
        AddFace(vertices, indices,
            new Vertex(width / 2f, 0f, length / 2f, sideUvs?.RightU ?? leftU, bottomV),
            new Vertex(width / 2f, 0f, -length / 2f, rightU, bottomV),
            new Vertex(width / 2f, height, -length / 2f, rightU, topV),
            new Vertex(width / 2f, height, length / 2f, sideUvs?.RightU ?? leftU, topV));

    private static void AddLeftFace(List<float> vertices, List<uint> indices, float width, float height, float length, float leftU, float rightU, float topV, float bottomV, PlaneSideUvs? sideUvs) =>
        AddFace(vertices, indices,
            new Vertex(-width / 2f, 0f, -length / 2f, sideUvs?.LeftU ?? rightU, bottomV),
            new Vertex(-width / 2f, 0f, length / 2f, leftU, bottomV),
            new Vertex(-width / 2f, height, length / 2f, leftU, topV),
            new Vertex(-width / 2f, height, -length / 2f, sideUvs?.LeftU ?? rightU, topV));

    private static void AddFace(List<float> vertices, List<uint> indices, Vertex a, Vertex b, Vertex c, Vertex d)
    {
        uint start = (uint)(vertices.Count / 5);
        AddVertex(vertices, a);
        AddVertex(vertices, b);
        AddVertex(vertices, c);
        AddVertex(vertices, d);
        indices.Add(start);
        indices.Add(start + 1);
        indices.Add(start + 2);
        indices.Add(start + 2);
        indices.Add(start + 3);
        indices.Add(start);
    }

    private static void AddVertex(List<float> vertices, Vertex vertex)
    {
        vertices.Add(vertex.X);
        vertices.Add(vertex.Y);
        vertices.Add(vertex.Z);
        vertices.Add(vertex.U);
        vertices.Add(vertex.V);
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
        _quadFlippedV?.Dispose();
        _quadFlippedV = null;
    }
}
