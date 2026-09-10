using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Linq;
using System;

namespace odl3d;

public class ObjFile
{
    public Dictionary<string, ObjGroup> Meshes;
    public string? MtlFilename;

    public ObjFile(Dictionary<string, ObjGroup> meshes, string? mtlFilename)
    {
        Meshes = meshes;
        MtlFilename = mtlFilename;
    }
}

public class ObjGroup
{
    public string Name;
    public string? MtlName;
    public Mesh Mesh;

    public ObjGroup(string name, string? mtlName, Mesh mesh)
    {
        Name = name;
        MtlName = mtlName;
        Mesh = mesh;
    }
}

public static class ObjLoader
{
    public static ObjFile Load(string filename)
    {
        List<Vector3> positions = new();
        List<Vector2> texCoords = new();

        ObjFile objFile = new ObjFile(new Dictionary<string, ObjGroup>(), null);

        string? currentName = "default";
        string? mtlName = null;

        List<float> vertices = new();
        List<uint> indices = new();
        Dictionary<string, uint> vertexLookup = new();

        void FinishCurrentMesh()
        {
            if (indices.Count == 0)
                return;
            objFile.Meshes[currentName] = new ObjGroup(currentName, mtlName, new Mesh(vertices.ToArray(), indices.ToArray()));
            vertices.Clear();
            indices.Clear();
            vertexLookup.Clear();
            mtlName = null;
        }

        foreach (string rawLine in File.ReadLines(filename))
        {
            string line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith("#"))
                continue;

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

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

                case "o":
                    FinishCurrentMesh();
                    currentName = string.Join(" ", parts.Skip(1));
                    break;

                // Also split per group
                case "g":
                    FinishCurrentMesh();
                    currentName = string.Join(" ", parts.Skip(1));
                    break;

                case "f":
                {
                    List<uint> faceIndices = new();

                    for (int i = 1; i < parts.Length; i++)
                    {
                        faceIndices.Add(
                            ResolveVertex(
                                parts[i],
                                positions,
                                texCoords,
                                vertices,
                                vertexLookup));
                    }

                    for (int i = 1; i < faceIndices.Count - 1; i++)
                    {
                        indices.Add(faceIndices[0]);
                        indices.Add(faceIndices[i]);
                        indices.Add(faceIndices[i + 1]);
                    }

                    break;
                }

                case "mtllib":
                    objFile.MtlFilename = string.Join(" ", parts.Skip(1));
                    break;

                case "usemtl":
                    mtlName = string.Join(" ", parts.Skip(1));
                    break;
            }
        }

        FinishCurrentMesh();

        return objFile;
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
