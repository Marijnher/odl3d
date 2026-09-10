using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

namespace odl3d.Loaders;

/// <summary>
/// Represents a material loaded from an MTL file, including its properties such as ambient, diffuse, and specular colors, shininess, dissolve factor, and associated textures.
/// </summary>
public class Material
{
    /// <summary>
    /// The name of the material as specified in the MTL file.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The ambient color of the material, represented as an array of three floats [r, g, b].
    /// </summary>
    public float[] Ka { get; set; } = { 0, 0, 0 };

    /// <summary>
    /// The diffuse color of the material, represented as an array of three floats [r, g, b].
    /// </summary>
    public float[] Kd { get; set; } = { 1, 1, 1 };

    /// <summary>
    /// The specular color of the material, represented as an array of three floats [r, g, b].
    /// </summary>
    public float[] Ks { get; set; } = { 0, 0, 0 };

    /// <summary>
    /// The shininess of the material.
    /// </summary>
    public float Ns { get; set; }

    /// <summary>
    /// The dissolve factor (transparency) of the material, corresponding to the 'd' or 'Tr' value in the MTL file.
    /// </summary>
    public float Dissolve { get; set; } = 1.0f;

    /// <summary>
    /// The file path to the diffuse texture (map_Kd) associated with the material.
    /// </summary>
    public string? DiffuseTexture { get; set; }

    /// <summary>
    /// The file path to the normal texture (map_Bump or bump) associated with the material.
    /// </summary>
    public string? NormalTexture { get; set; }

    /// <summary>
    /// The file path to the specular texture (map_Ks) associated with the material.
    /// </summary>
    public string? SpecularTexture { get; set; }   // map_Ks
}

/// <summary>
/// Provides functionality to load materials from an MTL file into a dictionary of Material objects.
/// </summary>
public static class MtlLoader
{
    /// <summary>
    /// Loads materials from the specified MTL file and returns them as a dictionary keyed by material name.
    /// </summary>
    /// <param name="path">The file path to the MTL file to be loaded.</param>
    /// <returns>A dictionary of Material objects keyed by their names.</returns>
    public static Dictionary<string, Material> Load(string path)
    {
        Dictionary<string, Material> materials = new();
        Material? current = null;

        foreach (string rawLine in File.ReadLines(path))
        {
            string line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            switch (parts[0])
            {
                case "newmtl":
                    current = new Material
                    {
                        Name = string.Join(" ", parts.Skip(1))
                    };
                    materials[current.Name] = current;
                    break;

                case "Ka":
                    current!.Ka = ParseVec3(parts);
                    break;

                case "Kd":
                    current!.Kd = ParseVec3(parts);
                    break;

                case "Ks":
                    current!.Ks = ParseVec3(parts);
                    break;

                case "Ns":
                    current!.Ns = ParseFloat(parts[1]);
                    break;

                case "d":
                    current!.Dissolve = ParseFloat(parts[1]);
                    break;

                case "Tr":
                    current!.Dissolve = 1.0f - ParseFloat(parts[1]);
                    break;

                case "map_Kd":
                    current!.DiffuseTexture = string.Join(" ", parts.Skip(1));
                    break;

                case "map_Ks":
                    current!.SpecularTexture = string.Join(" ", parts.Skip(1));
                    break;

                case "bump":
                case "map_Bump":
                    current!.NormalTexture = string.Join(" ", parts.Skip(1));
                    break;
            }
        }

        return materials;
    }

    /// <summary>
    /// Parses a Vec3 (three-component vector) from the given parts of a line in the MTL file.
    /// </summary>
    /// <param name="parts">The parts of the line containing the Vec3 components.</param>
    /// <returns>An array of three floats representing the Vec3.</returns>
    private static float[] ParseVec3(string[] parts)
    {
        return new[]
        {
            ParseFloat(parts[1]),
            ParseFloat(parts[2]),
            ParseFloat(parts[3])
        };
    }

    /// <summary>
    /// Parses a float from the given string using the invariant culture.
    /// </summary>
    /// <param name="s">The string to parse as a float.</param>
    /// <returns>The parsed float value.</returns>
    private static float ParseFloat(string s)
    {
        return float.Parse(s, CultureInfo.InvariantCulture);
    }
}