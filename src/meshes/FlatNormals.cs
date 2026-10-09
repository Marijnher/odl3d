using System.Collections.Generic;
using System.Numerics;

namespace odl3d;

/// <summary>
/// Generates flat (per-face) normals for geometry that does not supply them.
/// </summary>
internal static class FlatNormals
{
    /// <summary>
    /// Gives every triangle a normal if it lacks one. Triangles with normals are kept as they are; the others get their face normal, which requires their vertices to be separated from neighbouring triangles with a different normal.
    /// </summary>
    /// <param name="vertices">The source vertices.</param>
    /// <param name="indices">The triangle list indexing into <paramref name="vertices"/>.</param>
    /// <param name="hasNormals">True when the vertices already carry normals; only triangles with a missing (zero) normal are then rewritten.</param>
    /// <returns>The vertices and indices to use, and whether any normal was generated. The inputs are returned unchanged when nothing was generated.</returns>
    public static (Vertex[] Vertices, uint[] Indices, bool Generated) Apply(Vertex[] vertices, uint[] indices, bool hasNormals)
    {
        if (hasNormals && !AnyMissing(vertices, indices))
            return (vertices, indices, false);

        List<Vertex> outVertices = new(vertices.Length);
        Dictionary<Vertex, uint> lookup = new(vertices.Length);
        uint[] outIndices = new uint[indices.Length];
        bool generated = false;

        uint Add(Vertex vertex)
        {
            if (lookup.TryGetValue(vertex, out uint existing)) return existing;
            uint index = (uint) outVertices.Count;
            outVertices.Add(vertex);
            lookup[vertex] = index;
            return index;
        }

        int triangleEnd = indices.Length - indices.Length % 3;
        for (int i = 0; i < triangleEnd; i += 3)
        {
            Vertex a = vertices[indices[i]];
            Vertex b = vertices[indices[i + 1]];
            Vertex c = vertices[indices[i + 2]];
            if (hasNormals && HasNormal(a) && HasNormal(b) && HasNormal(c))
            {
                outIndices[i] = Add(a);
                outIndices[i + 1] = Add(b);
                outIndices[i + 2] = Add(c);
                continue;
            }

            Vector3 normal = Vector3.Cross(b.Position - a.Position, c.Position - a.Position);
            normal = normal.LengthSquared() > 1e-20f ? Vector3.Normalize(normal) : Vector3.UnitY;
            a.Normal = normal;
            b.Normal = normal;
            c.Normal = normal;
            outIndices[i] = Add(a);
            outIndices[i + 1] = Add(b);
            outIndices[i + 2] = Add(c);
            generated = true;
        }
        for (int i = triangleEnd; i < indices.Length; i++)
            outIndices[i] = Add(vertices[indices[i]]);

        return (outVertices.ToArray(), outIndices, generated);
    }

    private static bool HasNormal(Vertex vertex) => vertex.Normal.LengthSquared() > 1e-12f;

    private static bool AnyMissing(Vertex[] vertices, uint[] indices)
    {
        foreach (uint index in indices)
            if (!HasNormal(vertices[index])) return true;
        return false;
    }
}
