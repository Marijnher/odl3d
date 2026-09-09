using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace odl3d;

/// <summary>Loads a Wavefront .obj file (positions + texcoords, triangulating n-gon faces) into a Mesh.</summary>
public static class ObjLoader
{
    /// <summary>
    /// Loads a Wavefront .obj file from the specified path and returns a Mesh containing the vertex positions, texture coordinates, and indices. The loader supports triangulating faces with more than three vertices (n-gons) into triangles. It reads the vertex positions (v) and texture coordinates (vt) from the .obj file, resolves the vertex indices for each face, and constructs the final vertex and index buffers for the Mesh. The resulting Mesh can be used for rendering in a 3D scene.
    /// </summary>
    /// <param name="filename">The file path to the Wavefront .obj file to be loaded.</param>
    /// <returns>A Mesh containing the vertex positions, texture coordinates, and indices extracted from the .obj file.</returns>
    public static Mesh Load(string filename)
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector2> texCoords = new List<Vector2>();
        List<float> vertices = new List<float>();
        List<uint> indices = new List<uint>();
        Dictionary<string, uint> vertexLookup = new Dictionary<string, uint>();

        foreach (string rawLine in File.ReadAllLines(filename))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;

            string[] parts = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0])
            {
                case "v":
                    positions.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                    break;
                case "vt":
                    texCoords.Add(new Vector2(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture)));
                    break;
                case "f":
                    // fan-triangulate faces with more than 3 vertices
                    List<uint> faceIndices = new List<uint>();
                    for (int i = 1; i < parts.Length; i++)
                        faceIndices.Add(ResolveVertex(parts[i], positions, texCoords, vertices, vertexLookup));
                    for (int i = 1; i < faceIndices.Count - 1; i++)
                    {
                        indices.Add(faceIndices[0]);
                        indices.Add(faceIndices[i]);
                        indices.Add(faceIndices[i + 1]);
                    }
                    break;
            }
        }

        return new Mesh(vertices.ToArray(), indices.ToArray());
    }

    /// <summary>
    /// Resolves a vertex token from a face definition in the .obj file, extracting the position and texture coordinate indices. If the vertex has already been processed, it returns the existing index; otherwise, it adds the new vertex data to the vertices list and updates the vertex lookup dictionary. The method handles both positive and negative indices, allowing for proper referencing of positions and texture coordinates in the .obj file format. The resulting index is used to construct the final index buffer for the Mesh.
    /// </summary>
    /// <param name="token">The vertex token from the face definition (e.g., "1/2/3").</param>
    /// <param name="positions">The list of vertex positions parsed from the .obj file.</param>
    /// <param name="texCoords">The list of texture coordinates parsed from the .obj file.</param>
    /// <param name="vertices">The list of vertex data to be populated for the Mesh.</param>
    /// <param name="vertexLookup">A dictionary mapping vertex tokens to their corresponding indices in the vertices list.</param>
    /// <returns>The index of the resolved vertex in the vertices list.</returns>
    private static uint ResolveVertex(string token, List<Vector3> positions, List<Vector2> texCoords, List<float> vertices, Dictionary<string, uint> vertexLookup)
    {
        if (vertexLookup.TryGetValue(token, out uint existingIndex)) return existingIndex;

        string[] parts = token.Split('/');
        int positionIndex = int.Parse(parts[0], CultureInfo.InvariantCulture);
        positionIndex = positionIndex > 0 ? positionIndex - 1 : positions.Count + positionIndex;
        Vector3 position = positions[positionIndex];

        Vector2 texCoord = Vector2.Zero;
        if (parts.Length > 1 && parts[1].Length > 0)
        {
            int texCoordIndex = int.Parse(parts[1], CultureInfo.InvariantCulture);
            texCoordIndex = texCoordIndex > 0 ? texCoordIndex - 1 : texCoords.Count + texCoordIndex;
            texCoord = texCoords[texCoordIndex];
        }

        uint newIndex = (uint) (vertices.Count / 5);
        vertices.Add(position.X);
        vertices.Add(position.Y);
        vertices.Add(position.Z);
        vertices.Add(texCoord.X);
        vertices.Add(1f - texCoord.Y); // OBJ texcoords are bottom-up; flip to match our top-left convention
        vertexLookup[token] = newIndex;
        return newIndex;
    }
}
