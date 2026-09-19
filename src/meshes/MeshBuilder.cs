using System;
using System.Numerics;
using System.Collections.Generic;

namespace odl3d;

public class MeshBuilder
{
    public List<Vertex> Vertices { get; private set; } = new();
    public List<uint> Indices { get; private set; } = new();

    public void AddVertex(Vertex vertex)
    {
        Vertices.Add(vertex);
    }

    public void AddFace(Vertex v1, Vertex v2, Vertex v3)
    {
        if (!Vertices.Contains(v1)) AddVertex(v1);
        if (!Vertices.Contains(v2)) AddVertex(v2);
        if (!Vertices.Contains(v3)) AddVertex(v3);
        Indices.Add((uint) Vertices.IndexOf(v1));
        Indices.Add((uint) Vertices.IndexOf(v2));
        Indices.Add((uint) Vertices.IndexOf(v3));
    }

    public void AddFace(uint i1, uint i2, uint i3)
    {
        Indices.Add(i1);
        Indices.Add(i2);
        Indices.Add(i3);
    }

    public void AddQuad(Vertex s1, Vertex s2, Vertex s3, Vertex s4, float u = 1, float v = 1)
    {
        var v1 = new Vertex(s1.Position, 0, 0);
        var v2 = new Vertex(s2.Position, u, 0);
        var v3 = new Vertex(s3.Position, v, v);
        var v4 = new Vertex(s4.Position, 0, v);
        AddFace(v1, v2, v3);
        AddFace(v1, v3, v4);
    }

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

public class Vertex
{
    public Vector3 Position { get; set; }
    public Vector2 TexCoord { get; set; }
    public Vector3 Normal { get; set; }

    public Vertex(Vector3 position, Vector2 texCoord)
    {
        Position = position;
        TexCoord = texCoord;
    }

    public Vertex(Vector3 position, float u, float v)
    {
        Position = position;
        TexCoord = new Vector2(u, v);
    }

    public Vertex(Vector3 position, Vector3 normal, float u = 0, float v = 0) :
        this(position, new Vector2(u, v))
    {
        Normal = normal;
    }

    public Vertex(float x, float y, float z, float u = 0, float v = 0) :
        this(new Vector3(x, y, z), new Vector2(u, v)) { }
}