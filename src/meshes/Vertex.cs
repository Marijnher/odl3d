using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace odl3d;

/// <summary>
/// Represents a blittable vertex in 3D space, including position, texture coordinates, and a normal vector.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Vertex : IEquatable<Vertex>
{
    /// <summary>
    /// Gets or sets the position of the vertex in 3D space.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// Gets or sets the texture coordinates of the vertex.
    /// </summary>
    public Vector2 TexCoord;

    /// <summary>
    /// Gets or sets the normal vector of the vertex.
    /// </summary>
    public Vector3 Normal;

    /// <summary>
    /// Initializes a new instance of the Vertex struct with the specified position and texture coordinates.
    /// </summary>
    /// <param name="position">The position of the vertex in 3D space.</param>
    /// <param name="texCoord">The texture coordinates of the vertex.</param>
    public Vertex(Vector3 position, Vector2 texCoord)
    {
        Position = position;
        TexCoord = texCoord;
        Normal = Vector3.Zero;
    }

    /// <summary>
    /// Initializes a new instance of the Vertex struct with the specified position and texture coordinates given as separate u and v components.
    /// </summary>
    /// <param name="position">The position of the vertex in 3D space.</param>
    /// <param name="u">The horizontal texture coordinate of the vertex.</param>
    /// <param name="v">The vertical texture coordinate of the vertex.</param>
    public Vertex(Vector3 position, float u, float v)
        : this(position, new Vector2(u, v))
    {
    }

    /// <summary>
    /// Initializes a new instance of the Vertex struct with the specified position, normal vector, and optional texture coordinates.
    /// </summary>
    /// <param name="position">The position of the vertex in 3D space.</param>
    /// <param name="normal">The normal vector of the vertex.</param>
    /// <param name="u">The horizontal texture coordinate of the vertex.</param>
    /// <param name="v">The vertical texture coordinate of the vertex.</param>
    public Vertex(Vector3 position, Vector3 normal, float u = 0, float v = 0) :
        this(position, new Vector2(u, v))
    {
        Normal = normal;
    }

    /// <summary>
    /// Initializes a new instance of the Vertex struct with the specified position given as separate x, y, and z components, and optional texture coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the vertex position in 3D space.</param>
    /// <param name="y">The y-coordinate of the vertex position in 3D space.</param>
    /// <param name="z">The z-coordinate of the vertex position in 3D space.</param>
    /// <param name="u">The horizontal texture coordinate of the vertex.</param>
    /// <param name="v">The vertical texture coordinate of the vertex.</param>
    public Vertex(float x, float y, float z, float u = 0, float v = 0) :
        this(new Vector3(x, y, z), new Vector2(u, v)) { }

    public readonly bool Equals(Vertex other) =>
        Position.Equals(other.Position) &&
        TexCoord.Equals(other.TexCoord) &&
        Normal.Equals(other.Normal);

    public override readonly bool Equals(object? obj) => obj is Vertex other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(Position, TexCoord, Normal);
}