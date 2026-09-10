using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace odl3d.Loaders;

/// <summary>
/// Provides functionality for loading 3D models from DAE (Collada) files, including parsing meshes, materials, and textures.
/// </summary>
public static class DaeLoader
{
    private readonly record struct VertexKey(int Position, int TexCoord);

    private sealed class SourceData
    {
        public float[] Values = [];
        public int Stride = 1;
    }

    private sealed class VerticesData
    {
        public string? PositionSource;
        public string? TexCoordSource;
    }

    private sealed class InputData
    {
        public string Semantic = "";
        public string Source = "";
        public int Offset;
    }

    private sealed class MaterialData
    {
        public string? TexturePath;
        public TextureWrap WrapS = TextureWrap.Repeat;
        public TextureWrap WrapT = TextureWrap.Repeat;
        public bool Transparent;
    }

    private sealed class LoadedPart
    {
        public Mesh Mesh = null!;
        public Texture? Texture;
        public bool Transparent;
    }

    /// <summary>
    /// Loads a 3D model from a DAE (Collada) file, including its meshes and textures.
    /// </summary>
    /// <param name="filename">The path to the DAE file to load.</param>
    /// <param name="textureFolder">The folder containing texture files. If null, the folder of the DAE file is used.</param>
    /// <param name="scale">A scale factor to apply to the model's vertices.</param>
    /// <remarks>
    /// The returned arrays are ordered such that opaque parts come before transparent parts.
    /// </remarks>
    /// <returns>A tuple containing an array of loaded meshes and an array of corresponding textures.</returns>
    public static (Mesh[] Meshes, Texture?[] Textures) Load(string filename, string? textureFolder = null, float scale = 1f)
    {
        XDocument document = XDocument.Load(filename);
        string baseFolder = textureFolder ?? Path.GetDirectoryName(filename) ?? ".";
        Dictionary<string, MaterialData> materials = ReadMaterials(document);
        Dictionary<string, string> materialBindings = ReadMaterialBindings(document);
        List<LoadedPart> parts = [];

        foreach (XElement meshElement in document.Descendants().Where(x => x.Name.LocalName == "mesh"))
        {
            parts.AddRange(ReadMesh(meshElement, materials, materialBindings, baseFolder, scale));
        }

        LoadedPart[] orderedParts = parts
            .OrderBy(part => part.Transparent ? 1 : 0)
            .ToArray();

        return
        (
            orderedParts.Select(part => part.Mesh).ToArray(),
            orderedParts.Select(part => part.Texture).ToArray()
        );
    }

    private static IEnumerable<LoadedPart> ReadMesh(
        XElement meshElement,
        Dictionary<string, MaterialData> materials,
        Dictionary<string, string> materialBindings,
        string textureFolder,
        float scale)
    {
        Dictionary<string, SourceData> sources = meshElement.Elements()
            .Where(x => x.Name.LocalName == "source")
            .ToDictionary(source => RequiredAttribute(source, "id"), ReadSource);

        Dictionary<string, VerticesData> vertices = meshElement.Elements()
            .Where(x => x.Name.LocalName == "vertices")
            .ToDictionary(x => RequiredAttribute(x, "id"), ReadVertices);

        foreach (XElement primitive in meshElement.Elements().Where(IsSupportedPrimitive))
        {
            string materialSymbol = (string?)primitive.Attribute("material") ?? "";
            string materialId = materialBindings.TryGetValue(materialSymbol, out string? boundMaterialId)
                ? boundMaterialId
                : materialSymbol;
            materials.TryGetValue(materialId, out MaterialData? material);

            (float[] meshVertices, uint[] indices) = BuildPrimitive(primitive, sources, vertices, scale);

            yield return new LoadedPart
            {
                Mesh = new Mesh(meshVertices, indices),
                Texture = material?.TexturePath == null ? null : LoadTexture(textureFolder, material),
                Transparent = material?.Transparent ?? false
            };
        }
    }

    private static (float[] Vertices, uint[] Indices) BuildPrimitive(
        XElement primitive,
        Dictionary<string, SourceData> sources,
        Dictionary<string, VerticesData> vertices,
        float scale)
    {
        InputData[] inputs = primitive.Elements()
            .Where(x => x.Name.LocalName == "input")
            .Select(ReadInput)
            .ToArray();

        int stride = inputs.Max(input => input.Offset) + 1;
        InputData vertexInput = inputs.FirstOrDefault(input => input.Semantic == "VERTEX")
            ?? throw new FileLoadException("DAE primitive does not contain a VERTEX input.");

        if (!vertices.TryGetValue(vertexInput.Source, out VerticesData? vertexSources) || vertexSources.PositionSource == null)
        {
            throw new FileLoadException("DAE primitive references missing vertex position data.");
        }

        InputData? texCoordInput = inputs.FirstOrDefault(input => input.Semantic == "TEXCOORD");
        string? texCoordSource = texCoordInput?.Source ?? vertexSources.TexCoordSource;
        int texCoordOffset = texCoordInput?.Offset ?? vertexInput.Offset;

        if (texCoordSource == null)
        {
            throw new FileLoadException("DAE mesh does not have texture coordinates.");
        }

        SourceData positions = sources[vertexSources.PositionSource];
        SourceData texCoords = sources[texCoordSource];
        int[] cornerData = ReadCornerData(primitive);
        List<int[]> polygons = ReadPolygons(primitive, cornerData, stride);
        Dictionary<VertexKey, uint> vertexLookup = [];
        List<float> meshVertices = [];
        List<uint> indices = [];

        foreach (int[] polygon in polygons)
        {
            for (int i = 1; i < polygon.Length - 1; i++)
            {
                AddCorner(polygon[0]);
                AddCorner(polygon[i]);
                AddCorner(polygon[i + 1]);
            }
        }

        return (meshVertices.ToArray(), indices.ToArray());

        void AddCorner(int cornerOffset)
        {
            int positionIndex = cornerData[cornerOffset + vertexInput.Offset];
            int texCoordIndex = cornerData[cornerOffset + texCoordOffset];
            VertexKey key = new(positionIndex, texCoordIndex);

            if (!vertexLookup.TryGetValue(key, out uint index))
            {
                index = (uint)vertexLookup.Count;
                vertexLookup[key] = index;

                int positionOffset = positionIndex * positions.Stride;
                int texCoordDataOffset = texCoordIndex * texCoords.Stride;

                meshVertices.Add(positions.Values[positionOffset] / 50f * scale);
                meshVertices.Add(positions.Values[positionOffset + 1] / 50f * scale);
                meshVertices.Add(positions.Values[positionOffset + 2] / 50f * scale);
                meshVertices.Add(texCoords.Values[texCoordDataOffset]);
                meshVertices.Add(1f - texCoords.Values[texCoordDataOffset + 1]);
            }

            indices.Add(index);
        }
    }

    private static List<int[]> ReadPolygons(XElement primitive, int[] cornerData, int stride)
    {
        List<int[]> polygons = [];

        switch (primitive.Name.LocalName)
        {
            case "triangles":
                for (int i = 0; i < cornerData.Length; i += stride * 3)
                {
                    polygons.Add([i, i + stride, i + stride * 2]);
                }
                break;

            case "polylist":
                int[] counts = ParseInts(primitive.Elements().First(x => x.Name.LocalName == "vcount").Value);
                int cursor = 0;
                foreach (int count in counts)
                {
                    int[] polygon = new int[count];
                    for (int i = 0; i < count; i++)
                    {
                        polygon[i] = cursor;
                        cursor += stride;
                    }
                    polygons.Add(polygon);
                }
                break;

            case "polygons":
                int polygonCursor = 0;
                foreach (XElement polygonElement in primitive.Elements().Where(x => x.Name.LocalName == "p"))
                {
                    int polygonCornerCount = ParseInts(polygonElement.Value).Length / stride;
                    int[] polygon = new int[polygonCornerCount];
                    for (int i = 0; i < polygonCornerCount; i++)
                    {
                        polygon[i] = polygonCursor;
                        polygonCursor += stride;
                    }
                    polygons.Add(polygon);
                }
                break;
        }

        return polygons;
    }

    private static Dictionary<string, MaterialData> ReadMaterials(XDocument document)
    {
        Dictionary<string, string> images = document.Descendants()
            .Where(x => x.Name.LocalName == "image" && x.Attribute("id") != null)
            .ToDictionary(
                x => RequiredAttribute(x, "id"),
                x => x.Elements().FirstOrDefault(e => e.Name.LocalName == "init_from")?.Value ?? "");

        Dictionary<string, MaterialData> effects = document.Descendants()
            .Where(x => x.Name.LocalName == "effect" && x.Attribute("id") != null)
            .ToDictionary(x => RequiredAttribute(x, "id"), x => ReadEffect(x, images));

        Dictionary<string, MaterialData> materials = [];
        foreach (XElement material in document.Descendants().Where(x => x.Name.LocalName == "material" && x.Attribute("id") != null))
        {
            string materialId = RequiredAttribute(material, "id");
            string? effectId = material.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "instance_effect")?
                .Attribute("url")?
                .Value
                .TrimStart('#');

            if (effectId != null && effects.TryGetValue(effectId, out MaterialData? effect))
            {
                materials[materialId] = effect;
            }
        }

        return materials;
    }

    private static MaterialData ReadEffect(XElement effect, Dictionary<string, string> images)
    {
        Dictionary<string, XElement> parameters = effect.Descendants()
            .Where(x => x.Name.LocalName == "newparam" && x.Attribute("sid") != null)
            .ToDictionary(x => RequiredAttribute(x, "sid"), x => x);

        string? samplerSid = effect.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "diffuse")?
            .Elements()
            .FirstOrDefault(x => x.Name.LocalName == "texture")?
            .Attribute("texture")?
            .Value;

        MaterialData result = new()
        {
            Transparent = effect.Descendants().Any(x => x.Name.LocalName == "transparent")
        };

        if (samplerSid == null || !parameters.TryGetValue(samplerSid, out XElement? samplerParameter))
        {
            return result;
        }

        XElement? sampler = samplerParameter.Elements().FirstOrDefault(x => x.Name.LocalName == "sampler2D");
        string? surfaceSid = sampler?.Elements().FirstOrDefault(x => x.Name.LocalName == "source")?.Value;
        if (sampler == null || surfaceSid == null || !parameters.TryGetValue(surfaceSid, out XElement? surfaceParameter))
        {
            return result;
        }

        string? imageId = surfaceParameter.Descendants().FirstOrDefault(x => x.Name.LocalName == "init_from")?.Value;
        if (imageId == null || !images.TryGetValue(imageId, out string? texturePath) || string.IsNullOrWhiteSpace(texturePath))
        {
            return result;
        }

        result.TexturePath = texturePath;
        result.WrapS = ParseWrap(sampler.Elements().FirstOrDefault(x => x.Name.LocalName == "wrap_s")?.Value);
        result.WrapT = ParseWrap(sampler.Elements().FirstOrDefault(x => x.Name.LocalName == "wrap_t")?.Value);
        return result;
    }

    private static Dictionary<string, string> ReadMaterialBindings(XDocument document)
    {
        Dictionary<string, string> bindings = [];

        foreach (XElement instanceMaterial in document.Descendants().Where(x => x.Name.LocalName == "instance_material"))
        {
            string? symbol = (string?)instanceMaterial.Attribute("symbol");
            string? target = (string?)instanceMaterial.Attribute("target");
            if (symbol != null && target != null)
            {
                bindings[symbol] = target.TrimStart('#');
            }
        }

        return bindings;
    }

    private static SourceData ReadSource(XElement source)
    {
        XElement floatArray = source.Descendants().First(x => x.Name.LocalName == "float_array");
        XElement? accessor = source.Descendants().FirstOrDefault(x => x.Name.LocalName == "accessor");

        return new SourceData
        {
            Values = ParseFloats(floatArray.Value),
            Stride = (int?)accessor?.Attribute("stride") ?? 1
        };
    }

    private static VerticesData ReadVertices(XElement vertices)
    {
        VerticesData result = new();

        foreach (XElement input in vertices.Elements().Where(x => x.Name.LocalName == "input"))
        {
            string semantic = RequiredAttribute(input, "semantic");
            string source = RequiredAttribute(input, "source").TrimStart('#');

            if (semantic == "POSITION")
            {
                result.PositionSource = source;
            }
            else if (semantic == "TEXCOORD")
            {
                result.TexCoordSource = source;
            }
        }

        return result;
    }

    private static InputData ReadInput(XElement input)
    {
        return new InputData
        {
            Semantic = RequiredAttribute(input, "semantic"),
            Source = RequiredAttribute(input, "source").TrimStart('#'),
            Offset = (int?)input.Attribute("offset") ?? 0
        };
    }

    private static int[] ReadCornerData(XElement primitive)
    {
        return primitive.Name.LocalName == "polygons"
            ? primitive.Elements().Where(x => x.Name.LocalName == "p").SelectMany(x => ParseInts(x.Value)).ToArray()
            : ParseInts(primitive.Elements().First(x => x.Name.LocalName == "p").Value);
    }

    private static Texture LoadTexture(string textureFolder, MaterialData material)
    {
        string texturePath = Uri.UnescapeDataString(material.TexturePath!.Replace('/', Path.DirectorySeparatorChar));
        string filename = Path.IsPathRooted(texturePath) ? texturePath : Path.Combine(textureFolder, texturePath);

        return new Texture(filename)
        {
            WrapModeH = material.WrapS,
            WrapModeV = material.WrapT
        };
    }

    private static bool IsSupportedPrimitive(XElement element)
    {
        return element.Name.LocalName is "triangles" or "polylist" or "polygons";
    }

    private static TextureWrap ParseWrap(string? value)
    {
        return value?.ToUpperInvariant() switch
        {
            "WRAP" => TextureWrap.Repeat,
            "MIRROR" => TextureWrap.Mirror,
            "CLAMP" => TextureWrap.Clamp,
            "BORDER" => throw new FileLoadException("Border wrap mode is not supported."),
            _ => TextureWrap.Repeat
        };
    }

    private static float[] ParseFloats(string text)
    {
        return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(value => float.Parse(value, CultureInfo.InvariantCulture))
            .ToArray();
    }

    private static int[] ParseInts(string text)
    {
        return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(value => int.Parse(value, CultureInfo.InvariantCulture))
            .ToArray();
    }

    private static string RequiredAttribute(XElement element, string name)
    {
        return element.Attribute(name)?.Value
            ?? throw new FileLoadException($"DAE element '{element.Name.LocalName}' is missing required attribute '{name}'.");
    }
}