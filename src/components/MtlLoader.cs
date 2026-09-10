using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

public class Material
{
    public string Name { get; set; } = "";

    public float[] Ka { get; set; } = { 0, 0, 0 }; // Ambient
    public float[] Kd { get; set; } = { 1, 1, 1 }; // Diffuse
    public float[] Ks { get; set; } = { 0, 0, 0 }; // Specular

    public float Ns { get; set; }                  // Shininess
    public float Dissolve { get; set; } = 1.0f;    // d / Tr

    public string? DiffuseTexture { get; set; }    // map_Kd
    public string? NormalTexture { get; set; }     // map_Bump / bump
    public string? SpecularTexture { get; set; }   // map_Ks
}

public static class MtlLoader
{
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

    private static float[] ParseVec3(string[] parts)
    {
        return new[]
        {
            ParseFloat(parts[1]),
            ParseFloat(parts[2]),
            ParseFloat(parts[3])
        };
    }

    private static float ParseFloat(string s)
    {
        return float.Parse(s, CultureInfo.InvariantCulture);
    }
}