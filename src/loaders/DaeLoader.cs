using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using odl3d.Renderer;

namespace odl3d.Loaders;

/// <summary>
/// Provides functionality for loading 3D models from DAE (Collada) files, including parsing meshes, materials, and textures.
/// </summary>
public static class DaeLoader
{
    /// <summary>
    /// Gets or sets the default texture wrap mode for the S coordinate when loading textures from DAE files.
    /// </summary>
    public static TextureWrap DefaultTextureWrapS { get; set; } = TextureWrap.Repeat;

    /// <summary>
    /// Gets or sets the default texture wrap mode for the T coordinate when loading textures from DAE files.
    /// </summary>
    public static TextureWrap DefaultTextureWrapT { get; set; } = TextureWrap.Mirror;

    private readonly record struct VertexKey(int Position, int TexCoord, int Normal);

    private sealed class SourceData
    {
        public float[] Values = [];
        public int Stride = 1;
    }

    private sealed class VerticesData
    {
        public string? PositionSource;
        public string? TexCoordSource;
        public string? NormalSource;
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
        public TextureWrap WrapS = DefaultTextureWrapS;
        public TextureWrap WrapT = DefaultTextureWrapT;
        public bool Transparent;
        public SourceMaterial Lighting = new(Color.Black, Color.Black, 32f);
    }

    private sealed class LoadedPart
    {
        public Mesh Mesh = null!;
        public Texture? Texture;
        public Sampler? Sampler;
        public bool Transparent;
        public SourceMaterial Lighting = new(Color.Black, Color.Black, 32f);
        public Matrix4x4 LocalTransform = Matrix4x4.Identity;
    }

    private sealed class SceneInstance
    {
        public string GeometryId = "";
        public Dictionary<string, string> MaterialBindings = [];
        public Matrix4x4 Transform = Matrix4x4.Identity;
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
    public static (Mesh[] Meshes, Texture?[] Textures, Sampler?[] samplers, Matrix4x4[] LocalTransforms) Load(string filename, string? textureFolder = null, float scale = 1f)
    {
        DaeModelData data = LoadModelData(filename, textureFolder, scale);
        return (data.Meshes, data.Textures, data.Samplers, data.LocalTransforms);
    }

    /// <summary>
    /// Loads a DAE file like <see cref="Load"/>, additionally returning the per-part lighting properties and the lights defined in the file.
    /// </summary>
    public static DaeModelData LoadModelData(string filename, string? textureFolder = null, float scale = 1f)
    {
        XDocument document = XDocument.Load(filename);
        string baseFolder = textureFolder ?? Path.GetDirectoryName(filename) ?? ".";
        Dictionary<string, MaterialData> materials = ReadMaterials(document);
        Dictionary<string, string> controllerGeometryIds = ReadControllerGeometryIds(document);
        Dictionary<string, XElement> meshElements = document.Descendants()
            .Where(x => x.Name.LocalName == "mesh" && x.Parent?.Attribute("id") != null)
            .ToDictionary(x => RequiredAttribute(x.Parent!, "id"), x => x);
        List<LoadedPart> parts = [];
        List<SceneInstance> sceneInstances = ReadSceneInstances(document, controllerGeometryIds, scale);

        if (sceneInstances.Count > 0)
        {
            foreach (SceneInstance instance in sceneInstances)
            {
                if (meshElements.TryGetValue(instance.GeometryId, out XElement? meshElement))
                {
                    parts.AddRange(ReadMesh(meshElement, materials, instance.MaterialBindings, baseFolder, scale, instance.Transform));
                }
            }
        }
        else
        {
            Dictionary<string, Dictionary<string, string>> materialBindings = ReadMaterialBindings(document, controllerGeometryIds);

            foreach (KeyValuePair<string, XElement> meshEntry in meshElements)
            {
                Dictionary<string, string> meshMaterialBindings = materialBindings.TryGetValue(meshEntry.Key, out Dictionary<string, string>? bindings)
                    ? bindings
                    : [];

                parts.AddRange(ReadMesh(meshEntry.Value, materials, meshMaterialBindings, baseFolder, scale, Matrix4x4.Identity));
            }
        }

        LoadedPart[] orderedParts = parts
            .OrderBy(part => part.Transparent ? 1 : 0)
            .ToArray();

        return new DaeModelData(
            orderedParts.Select(part => part.Mesh).ToArray(),
            orderedParts.Select(part => part.Texture).ToArray(),
            orderedParts.Select(part => part.Sampler).ToArray(),
            orderedParts.Select(part => part.LocalTransform).ToArray(),
            orderedParts.Select(part => part.Lighting).ToArray(),
            ReadLights(document, scale).ToArray());
    }

    /// <summary>
    /// The result of loading a DAE file with <see cref="LoadModelData"/>. The part arrays are index-aligned and ordered with opaque parts first.
    /// </summary>
    public sealed record DaeModelData(Mesh[] Meshes, Texture?[] Textures, Sampler?[] Samplers, Matrix4x4[] LocalTransforms, SourceMaterial[] Materials, SourceLight[] Lights);

    private static List<SourceLight> ReadLights(XDocument document, float scale)
    {
        List<SourceLight> result = [];
        Dictionary<string, XElement> definitions = document.Descendants()
            .Where(x => x.Name.LocalName == "light" && x.Attribute("id") != null && x.Parent?.Name.LocalName == "library_lights")
            .ToDictionary(x => RequiredAttribute(x, "id"), x => x);
        XElement? visualScene = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "visual_scene");
        if (definitions.Count == 0 || visualScene == null) return result;

        foreach (XElement node in visualScene.Elements().Where(x => x.Name.LocalName == "node"))
            Visit(node, Matrix4x4.Identity);
        return result;

        void Visit(XElement node, Matrix4x4 parentTransform)
        {
            Matrix4x4 transform = ReadNodeTransform(node, scale) * parentTransform;
            foreach (XElement instance in node.Elements().Where(x => x.Name.LocalName == "instance_light"))
            {
                string? id = ((string?)instance.Attribute("url"))?.TrimStart('#');
                if (id != null && definitions.TryGetValue(id, out XElement? definition) && ReadLight(definition, transform) is SourceLight light)
                    result.Add(light);
            }
            foreach (XElement child in node.Elements().Where(x => x.Name.LocalName == "node"))
                Visit(child, transform);
        }
    }

    private static SourceLight? ReadLight(XElement definition, Matrix4x4 transform)
    {
        XElement? technique = definition.Descendants().FirstOrDefault(x => x.Name.LocalName == "technique_common");
        XElement? kind = technique?.Elements().FirstOrDefault();
        if (kind == null) return null;

        SourceLightKind? lightKind = kind.Name.LocalName switch
        {
            "point" => SourceLightKind.Point,
            "directional" => SourceLightKind.Directional,
            "spot" => SourceLightKind.Spot,
            _ => null
        };
        float[]? rgb = kind.Elements().FirstOrDefault(x => x.Name.LocalName == "color") is XElement color ? ParseFloats(color.Value) : null;
        if (lightKind == null || rgb == null || rgb.Length < 3) return null;

        float outerAngle = 45f;
        if (kind.Elements().FirstOrDefault(x => x.Name.LocalName == "falloff_angle") is XElement angle && ParseFloats(angle.Value) is { Length: > 0 } angleValues)
            outerAngle = Math.Clamp(angleValues[0] / 2f, 1f, 89f);

        return new SourceLight(lightKind.Value, SourceMaterial.ToColor(rgb[0], rgb[1], rgb[2]), transform, outerAngle);
    }

    private static IEnumerable<LoadedPart> ReadMesh(
        XElement meshElement,
        Dictionary<string, MaterialData> materials,
        Dictionary<string, string> materialBindings,
        string textureFolder,
        float scale,
        Matrix4x4 localTransform)
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

            (Vertex[] meshVertices, uint[] indices, bool hasNormals) = BuildPrimitive(primitive, sources, vertices, scale);

            (Texture? texture, Sampler? sampler) = material?.TexturePath == null ? (null, null) : LoadTexture(textureFolder, material);
            yield return new LoadedPart
            {
                Mesh = new Mesh(meshVertices, indices, hasNormals),
                Texture = texture,
                Sampler = sampler,
                Transparent = material?.Transparent ?? false,
                Lighting = material?.Lighting ?? new(Color.Black, Color.Black, 32f),
                LocalTransform = localTransform
            };
        }
    }

    private static (Vertex[] Vertices, uint[] Indices, bool HasNormals) BuildPrimitive(
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
        InputData? normalInput = inputs.FirstOrDefault(input => input.Semantic == "NORMAL");
        string? normalSource = normalInput?.Source ?? vertexSources.NormalSource;
        int normalOffset = normalInput?.Offset ?? vertexInput.Offset;

        if (texCoordSource == null)
        {
            throw new FileLoadException("DAE mesh does not have texture coordinates.");
        }

        SourceData positions = sources[vertexSources.PositionSource];
        SourceData texCoords = sources[texCoordSource];
        int[] cornerData = ReadCornerData(primitive);
        List<int[]> polygons = ReadPolygons(primitive, cornerData, stride);
        Dictionary<VertexKey, uint> vertexLookup = [];
        List<Vertex> meshVertices = [];
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

        return (meshVertices.ToArray(), indices.ToArray(), normalSource != null);

        void AddCorner(int cornerOffset)
        {
            int positionIndex = cornerData[cornerOffset + vertexInput.Offset];
            int texCoordIndex = cornerData[cornerOffset + texCoordOffset];
            int normalIndex = normalSource == null ? -1 : cornerData[cornerOffset + normalOffset];
            VertexKey key = new(positionIndex, texCoordIndex, normalIndex);

            if (!vertexLookup.TryGetValue(key, out uint index))
            {
                index = (uint)vertexLookup.Count;
                vertexLookup[key] = index;

                int positionOffset = positionIndex * positions.Stride;
                int texCoordDataOffset = texCoordIndex * texCoords.Stride;

                Vector3 position = new(
                    positions.Values[positionOffset] / 50f * scale,
                    positions.Values[positionOffset + 1] / 50f * scale,
                    positions.Values[positionOffset + 2] / 50f * scale);
                Vector3 normal = Vector3.Zero;
                if (normalSource != null)
                {
                    SourceData normalData = sources[normalSource];
                    int normalDataOffset = normalIndex * normalData.Stride;
                    normal = new Vector3(
                        normalData.Values[normalDataOffset],
                        normalData.Values[normalDataOffset + 1],
                        normalData.Values[normalDataOffset + 2]);
                }
                meshVertices.Add(new Vertex(
                    position,
                    normal,
                    texCoords.Values[texCoordDataOffset],
                    1f - texCoords.Values[texCoordDataOffset + 1]));
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
            Transparent = effect.Descendants().Any(x => x.Name.LocalName == "transparent"),
            Lighting = ReadEffectLighting(effect)
        };
        bool hasDiffuseTexture = samplerSid != null;
        if (hasDiffuseTexture)
        {
            // Exporters commonly write a white emission next to a diffuse texture, which would wash the texture out.
            result.Lighting = result.Lighting with { Emissive = Color.Black };
        }
        else
        {
            result.Lighting = result.Lighting with { Diffuse = ReadEffectColor(effect, "diffuse") };
        }

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
        result.WrapS = ParseWrap(sampler.Elements().FirstOrDefault(x => x.Name.LocalName == "wrap_s")?.Value, DefaultTextureWrapS);
        result.WrapT = ParseWrap(sampler.Elements().FirstOrDefault(x => x.Name.LocalName == "wrap_t")?.Value, DefaultTextureWrapT);
        return result;
    }

    private static SourceMaterial ReadEffectLighting(XElement effect)
    {
        Color emissive = ReadEffectColor(effect, "emission") ?? Color.Black;
        Color specular = ReadEffectColor(effect, "specular") ?? Color.Black;
        float shininess = 32f;
        XElement? shininessElement = effect.Descendants().FirstOrDefault(x => x.Name.LocalName == "shininess");
        if (shininessElement?.Descendants().FirstOrDefault(x => x.Name.LocalName == "float") is XElement value && ParseFloats(value.Value) is { Length: > 0 } values)
            shininess = MathF.Max(values[0], 1f);
        return new SourceMaterial(emissive, specular, shininess);
    }

    private static Color? ReadEffectColor(XElement effect, string property)
    {
        XElement? color = effect.Descendants().FirstOrDefault(x => x.Name.LocalName == property)?
            .Elements().FirstOrDefault(x => x.Name.LocalName == "color");
        if (color == null || ParseFloats(color.Value) is not { Length: >= 3 } rgb) return null;
        return SourceMaterial.ToColor(rgb[0], rgb[1], rgb[2]);
    }

    private static Dictionary<string, string> ReadControllerGeometryIds(XDocument document)
    {
        return document.Descendants()
            .Where(x => x.Name.LocalName == "controller" && x.Attribute("id") != null)
            .Select(controller => new
            {
                Id = RequiredAttribute(controller, "id"),
                GeometryId = controller.Descendants()
                    .FirstOrDefault(x => x.Name.LocalName == "skin")?
                    .Attribute("source")?
                    .Value
                    .TrimStart('#')
            })
            .Where(controller => controller.GeometryId != null)
            .ToDictionary(controller => controller.Id, controller => controller.GeometryId!);
    }

    private static List<SceneInstance> ReadSceneInstances(XDocument document, Dictionary<string, string> controllerGeometryIds, float scale)
    {
        List<SceneInstance> instances = [];
        XElement? visualScene = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "visual_scene");
        if (visualScene == null)
        {
            return instances;
        }

        foreach (XElement node in visualScene.Elements().Where(x => x.Name.LocalName == "node"))
        {
            ReadNodeInstances(node, Matrix4x4.Identity);
        }

        return instances;

        void ReadNodeInstances(XElement node, Matrix4x4 parentTransform)
        {
            Matrix4x4 nodeTransform = ReadNodeTransform(node, scale) * parentTransform;

            foreach (XElement instance in node.Elements().Where(x => x.Name.LocalName is "instance_geometry" or "instance_controller"))
            {
                string? url = (string?)instance.Attribute("url");
                if (url == null)
                {
                    continue;
                }

                string instanceId = url.TrimStart('#');
                string geometryId = instance.Name.LocalName == "instance_controller"
                    ? controllerGeometryIds.GetValueOrDefault(instanceId, instanceId)
                    : instanceId;

                instances.Add(new SceneInstance
                {
                    GeometryId = geometryId,
                    MaterialBindings = ReadInstanceMaterialBindings(instance),
                    Transform = nodeTransform
                });
            }

            foreach (XElement childNode in node.Elements().Where(x => x.Name.LocalName == "node"))
            {
                ReadNodeInstances(childNode, nodeTransform);
            }
        }
    }

    private static Matrix4x4 ReadNodeTransform(XElement node, float scale)
    {
        Matrix4x4 transform = Matrix4x4.Identity;

        foreach (XElement element in node.Elements())
        {
            transform = element.Name.LocalName switch
            {
                "matrix" => transform * ParseMatrix(element.Value, scale),
                "translate" => transform * ParseTranslation(element.Value, scale),
                _ => transform
            };
        }

        return transform;
    }

    private static Matrix4x4 ParseMatrix(string text, float scale)
    {
        float[] values = ParseFloats(text);
        if (values.Length != 16)
        {
            throw new FileLoadException("DAE matrix transform must contain 16 float values.");
        }

        return new Matrix4x4(
            values[0], values[4], values[8], values[12],
            values[1], values[5], values[9], values[13],
            values[2], values[6], values[10], values[14],
            values[3] / 50f * scale, values[7] / 50f * scale, values[11] / 50f * scale, values[15]);
    }

    private static Matrix4x4 ParseTranslation(string text, float scale)
    {
        float[] values = ParseFloats(text);
        if (values.Length < 3)
        {
            throw new FileLoadException("DAE translate transform must contain at least 3 float values.");
        }

        return Matrix4x4.CreateTranslation(values[0] / 50f * scale, values[1] / 50f * scale, values[2] / 50f * scale);
    }

    private static Dictionary<string, string> ReadInstanceMaterialBindings(XElement instance)
    {
        Dictionary<string, string> bindings = [];

        foreach (XElement instanceMaterial in instance.Descendants().Where(x => x.Name.LocalName == "instance_material"))
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

    private static Dictionary<string, Dictionary<string, string>> ReadMaterialBindings(XDocument document, Dictionary<string, string> controllerGeometryIds)
    {

        Dictionary<string, Dictionary<string, string>> bindings = [];

        foreach (XElement instance in document.Descendants().Where(x => x.Name.LocalName is "instance_geometry" or "instance_controller"))
        {
            string? url = (string?)instance.Attribute("url");
            if (url == null)
            {
                continue;
            }

            string instanceId = url.TrimStart('#');
            string geometryId = instance.Name.LocalName == "instance_controller"
                ? controllerGeometryIds.GetValueOrDefault(instanceId, instanceId)
                : instanceId;

            Dictionary<string, string> geometryBindings = bindings.TryGetValue(geometryId, out Dictionary<string, string>? existingBindings)
                ? existingBindings
                : bindings[geometryId] = [];

            foreach (XElement instanceMaterial in instance.Descendants().Where(x => x.Name.LocalName == "instance_material"))
            {
                string? symbol = (string?)instanceMaterial.Attribute("symbol");
                string? target = (string?)instanceMaterial.Attribute("target");
                if (symbol != null && target != null)
                {
                    geometryBindings[symbol] = target.TrimStart('#');
                }
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
            else if (semantic == "NORMAL")
            {
                result.NormalSource = source;
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

    private static (Texture, Sampler) LoadTexture(string textureFolder, MaterialData material)
    {
        string texturePath = Uri.UnescapeDataString(material.TexturePath!.Replace('/', Path.DirectorySeparatorChar));
        string filename = Path.IsPathRooted(texturePath) ? texturePath : Path.Combine(textureFolder, texturePath);

        Texture texture = new Texture(filename);
        Sampler sampler = new Sampler
        {
            WrapU = material.WrapS,
            WrapV = material.WrapT
        };

        return (texture, sampler);
    }

    private static bool IsSupportedPrimitive(XElement element)
    {
        return element.Name.LocalName is "triangles" or "polylist" or "polygons";
    }

    private static TextureWrap ParseWrap(string? value, TextureWrap defaultWrap)
    {
        return value?.ToUpperInvariant() switch
        {
            "WRAP" => TextureWrap.Repeat,
            "MIRROR" => TextureWrap.Mirror,
            "CLAMP" => TextureWrap.Clamp,
            "BORDER" => throw new FileLoadException("Border wrap mode is not supported."),
            _ => defaultWrap
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