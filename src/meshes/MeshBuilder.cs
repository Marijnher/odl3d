using System;
using System.Numerics;
using System.Collections.Generic;

namespace odl3d;

/// <summary>
/// Provides functionality for incrementally building a mesh by adding vertices and faces, and then constructing a Mesh object from the accumulated data.
/// </summary>
public class MeshBuilder
{
    /// <summary>
    /// Gets the list of vertices that have been added to the mesh builder.
    /// </summary>
    public List<Vertex> Vertices { get; private set; } = new();

    /// <summary>
    /// Gets the list of indices that have been added to the mesh builder.
    /// </summary>
    public List<uint> Indices { get; private set; } = new();

    /// <summary>
    /// Adds a vertex to the mesh builder's list of vertices.
    /// </summary>
    /// <param name="vertex">The vertex to add to the mesh builder.</param>
    public void AddVertex(Vertex vertex)
    {
        Vertices.Add(vertex);
    }

    /// <summary>
    /// Adds a triangular face to the mesh builder using the specified vertices. If any of the vertices are not already in the mesh builder's vertex list, they will be added. The indices of the vertices are then added to the index list to define the triangle.
    /// </summary>
    /// <param name="v1">The first vertex of the triangular face.</param>
    /// <param name="v2">The second vertex of the triangular face.</param>
    /// <param name="v3">The third vertex of the triangular face.</param>
    public void AddFace(Vertex v1, Vertex v2, Vertex v3)
    {
        if (!Vertices.Contains(v1)) AddVertex(v1);
        if (!Vertices.Contains(v2)) AddVertex(v2);
        if (!Vertices.Contains(v3)) AddVertex(v3);
        Indices.Add((uint) Vertices.IndexOf(v1));
        Indices.Add((uint) Vertices.IndexOf(v2));
        Indices.Add((uint) Vertices.IndexOf(v3));
    }

    /// <summary>
    /// Adds a triangular face to the mesh builder using the specified vertex indices.
    /// </summary>
    /// <param name="i1">The index of the first vertex of the triangular face.</param>
    /// <param name="i2">The index of the second vertex of the triangular face.</param>
    /// <param name="i3">The index of the third vertex of the triangular face.</param>
    public void AddFace(uint i1, uint i2, uint i3)
    {
        Indices.Add(i1);
        Indices.Add(i2);
        Indices.Add(i3);
    }

    /// <summary>
    /// Adds a quadrilateral face to the mesh builder using the specified vertices. The quad is defined by four vertices and is split into two triangular faces internally. The texture coordinates for the quad can be specified using the optional u and v parameters.
    /// </summary>
    /// <param name="s1">The first vertex of the quadrilateral face.</param>
    /// <param name="s2">The second vertex of the quadrilateral face.</param>
    /// <param name="s3">The third vertex of the quadrilateral face.</param>
    /// <param name="s4">The fourth vertex of the quadrilateral face.</param>
    /// <param name="u">The horizontal texture coordinate scale for the quad.</param>
    /// <param name="v">The vertical texture coordinate scale for the quad.</param>
    public void AddQuad(Vertex s1, Vertex s2, Vertex s3, Vertex s4, float u = 1, float v = 1)
    {
        var v1 = new Vertex(s1.Position, 0, 0);
        var v2 = new Vertex(s2.Position, u, 0);
        var v3 = new Vertex(s3.Position, v, v);
        var v4 = new Vertex(s4.Position, 0, v);
        AddFace(v1, v2, v3);
        AddFace(v1, v3, v4);
    }

    /// <summary>
    /// Builds and returns a Mesh object based on the current state of the mesh builder. The mesh will include vertex normals if specified by the hasNormals parameter.
    /// </summary>
    /// <param name="hasNormals">True if the mesh should include vertex normals.</param>
    /// <returns>The constructed Mesh object.</returns>
    public Mesh Build(bool hasNormals = false)
    {
        int floatsPerVertex = hasNormals ? 8 : 5;
        float[] vertices = new float[Vertices.Count * floatsPerVertex];
        for (int i = 0; i < Vertices.Count; i++)
        {
            Vertex v = Vertices[i];
            int offset = i * floatsPerVertex;
            vertices[offset] = v.Position.X;
            vertices[offset + 1] = v.Position.Y;
            vertices[offset + 2] = v.Position.Z;
            vertices[offset + 3] = v.TexCoord.X;
            vertices[offset + 4] = v.TexCoord.Y;
            if (hasNormals)
            {
                vertices[offset + 5] = v.Normal.X;
                vertices[offset + 6] = v.Normal.Y;
                vertices[offset + 7] = v.Normal.Z;
            }
        }
        return new Mesh(vertices, Indices.ToArray(), hasNormals);
    }

    /// <summary>
    /// Creates a textured quadrilateral mesh with the specified width, height, and optional texture coordinate scaling.
    /// </summary>
    /// <param name="w">The width of the quad.</param>
    /// <param name="h">The height of the quad.</param>
    /// <param name="u">The horizontal texture coordinate scale for the quad.</param>
    /// <param name="v">The vertical texture coordinate scale for the quad.</param>
    /// <returns>The constructed Mesh object representing the quad.</returns>
    public static Mesh CreateQuad(float w = 1, float h = 1, float u = 1, float v = 1)
    {
        MeshBuilder builder = new MeshBuilder();
        
        var bottomLeft  = new Vertex(0, 0, 0,   0, 0);
        var bottomRight = new Vertex(w, 0, 0,   u, 0);
        var topLeft     = new Vertex(0, h, 0,   0, v);
        var topRight    = new Vertex(w, h, 0,   u, v);

        builder.AddFace(bottomLeft, bottomRight, topRight);
        builder.AddFace(bottomLeft, topRight, topLeft);

        return builder.Build();
    }

    /// <summary>
    /// Creates a textured plane mesh with the specified width, height, depth, and optional texture coordinate scaling.
    /// </summary>
    /// <param name="w">The width of the plane.</param>
    /// <param name="h">The height of the plane.</param>
    /// <param name="d">The depth of the plane.</param>
    /// <param name="u">The horizontal texture coordinate scale for the plane.</param>
    /// <param name="v">The vertical texture coordinate scale for the plane.</param>
    /// <returns>The constructed Mesh object representing the plane.</returns>
    public static Mesh CreatePlane(float w, float h, float d, float u = 1, float v = 1)
    {
        MeshBuilder builder = new MeshBuilder();

        var frontBottomLeft     = new Vertex(0, 0, 0);
        var frontBottomRight    = new Vertex(w, 0, 0);
        var frontTopLeft        = new Vertex(0, h, 0);
        var frontTopRight       = new Vertex(w, h, 0);

        var backBottomLeft      = new Vertex(0, 0, d);
        var backBottomRight     = new Vertex(w, 0, d);
        var backTopLeft         = new Vertex(0, h, d);
        var backTopRight        = new Vertex(w, h, d);

        builder.AddQuad(frontBottomLeft, frontBottomRight, frontTopRight, frontTopLeft, u, v); // Front
        builder.AddQuad(backBottomLeft, frontBottomLeft, frontTopLeft, backTopLeft, u, v); // Left
        builder.AddQuad(frontBottomRight, backBottomRight, backTopRight, frontTopRight, u, v); // Right
        builder.AddQuad(frontTopLeft, frontTopRight, backTopRight, backTopLeft, u, v); // Top
        builder.AddQuad(frontBottomLeft, frontBottomRight, backBottomRight, backBottomLeft, u, v); // Bottom
        builder.AddQuad(backBottomLeft, backBottomRight, backTopRight, backTopLeft, u, v); // Back

        return builder.Build();
    }

    /// <summary>
    /// Creates a UV sphere centered at the origin.
    /// </summary>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="resolution">The number of subdivisions around the sphere and from pole to pole.</param>
    public static Mesh CreateSphere(float radius = 1, int resolution = 16)
    {
        if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius), "The radius must be greater than zero.");
        if (resolution < 3) throw new ArgumentOutOfRangeException(nameof(resolution), "The resolution must be at least 3.");

        MeshBuilder builder = new MeshBuilder();
        int verticesPerRow = resolution + 1;

        for (int row = 0; row <= resolution; row++)
        {
            float v = (float) row / resolution;
            float phi = MathF.PI * v;
            float y = MathF.Cos(phi) * radius;
            float ringRadius = MathF.Sin(phi) * radius;

            for (int column = 0; column <= resolution; column++)
            {
                float u = (float) column / resolution;
                float theta = MathF.Tau * u;
                Vector3 position = new(
                    ringRadius * MathF.Cos(theta),
                    y,
                    ringRadius * MathF.Sin(theta));
                builder.AddVertex(new Vertex(position, new Vector3(
                    position.X / radius,
                    position.Y / radius,
                    position.Z / radius), u, v));
            }
        }

        for (int row = 0; row < resolution; row++)
        {
            for (int column = 0; column < resolution; column++)
            {
                uint topLeft = (uint) (row * verticesPerRow + column);
                uint topRight = topLeft + 1;
                uint bottomLeft = topLeft + (uint) verticesPerRow;
                uint bottomRight = bottomLeft + 1;

                // Keep the winding order facing away from the sphere's center.
                builder.AddFace(topLeft, topRight, bottomLeft);
                builder.AddFace(bottomLeft, topRight, bottomRight);
            }
        }

        return builder.Build(true);
    }
}