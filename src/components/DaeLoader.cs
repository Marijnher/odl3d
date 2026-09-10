using System;
using Assimp;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace odl3d;

/// <summary>
/// Provides functionality to load meshes and textures from a COLLADA (DAE) file, including handling texture wrap modes.
/// </summary>
public static class DaeLoader
{
    /// <summary>
    /// Loads meshes and textures from a COLLADA (DAE) file, applying the specified scale to the vertex positions and handling texture wrap modes.
    /// </summary>
    /// <param name="filename">The path to the COLLADA (DAE) file to load.</param>
    /// <param name="textureFolder">The folder containing the textures referenced by the DAE file.</param>
    /// <param name="scale">A scale factor to apply to the vertex positions.</param>
    /// <exception cref="FileLoadException">Thrown if the wrap values for a texture are not found in the DAE file.</exception>
    /// <exception cref="FileLoadException">Thrown if the wrap mode is "BORDER", which is not supported.</exception>
    /// <exception cref="FileLoadException">Thrown if the texture file for an image ID is not found.</exception>
    /// <exception cref="FileLoadException">Thrown if the mesh does not have texture coordinates.</exception>
    /// <returns>A tuple containing an array of loaded meshes and an array of corresponding textures.</returns>
    public static (Mesh[], Texture[]) Load(string filename, string textureFolder, float scale = 1f)
    {
        using var importer = new AssimpContext();

        // Load and process the DAE file
        var mScene = importer.ImportFile(
            filename,
            PostProcessSteps.Triangulate |
            PostProcessSteps.FlipUVs
        );
        // Load the file again to extract only wrap_s and wrap_t values
        var wrapValues = DaeWrapLoader.LoadWrapData(filename);
        // TODO: Create own DAE parser to handle wrap modes directly

        Mesh[] meshes = new Mesh[mScene.MeshCount];
        Texture[] textures = new Texture[mScene.MeshCount];
        int startIdx = 0;
        int eindIdx = meshes.Length - 1;

        for (int m = 0; m < mScene.MeshCount; m++)
        {
            Assimp.Mesh mMesh = mScene.Meshes[m];
            float[] vertices = new float[mMesh.VertexCount * 5];
            uint[] indices = new uint[mMesh.FaceCount * 3];
            for (int i = 0; i < mMesh.FaceCount; i++)
            {
                indices[i * 3 + 0] = (uint) mMesh.Faces[i].Indices[0];
                indices[i * 3 + 1] = (uint) mMesh.Faces[i].Indices[1];
                indices[i * 3 + 2] = (uint) mMesh.Faces[i].Indices[2];
            }
            if (!mMesh.HasTextureCoords(0))
            {
                throw new FileLoadException("Mesh does not have texture coordinates.");
            }
            for (int i = 0; i < mMesh.VertexCount; i++)
            {
                vertices[i * 5 + 0] = mMesh.Vertices[i].X / 50f * scale;
                vertices[i * 5 + 1] = mMesh.Vertices[i].Y / 50f * scale;
                vertices[i * 5 + 2] = mMesh.Vertices[i].Z / 50f * scale;
                vertices[i * 5 + 3] = mMesh.TextureCoordinateChannels[0][i].X;
                vertices[i * 5 + 4] = mMesh.TextureCoordinateChannels[0][i].Y;
            }
            // Create an Object for this mesh
            Assimp.Material mat = mScene.Materials[mMesh.MaterialIndex];
            // Order the meshes/textures in such a way that all transparent meshes are at the end
            int idx;
            if (mat.HasOpacity)
            {
                idx = eindIdx;
                eindIdx--;
            }
            else
            {
                idx = startIdx;
                startIdx++;
            }
            string texFilename = mat.TextureDiffuse.FilePath;
            meshes[idx] = new Mesh(vertices, indices);
            if (!mat.HasTextureDiffuse) continue;
            string wrapName = texFilename.Replace(" ", "%20");
            if (!wrapValues.ContainsKey(wrapName))
            {
                throw new FileLoadException("Wrap values not found for texture: " + wrapName);
            }
            (TextureWrap wrapH, TextureWrap wrapV) = wrapValues[wrapName];
            textures[idx] = new Texture(textureFolder + "/" + texFilename)
            {
                WrapModeH = wrapH,
                WrapModeV = wrapV
            };
        }

        return (meshes, textures);
    }
}

/// <summary>
/// Loads texture wrap mode data from a COLLADA (DAE) file, mapping texture filenames to their corresponding wrap modes.
/// </summary>
class DaeWrapLoader
{
    /// <summary>
    /// Loads texture wrap mode data from the specified COLLADA (DAE) file.
    /// </summary>
    /// <param name="filename">The path to the COLLADA (DAE) file containing texture wrap mode information.</param>
    /// <exception cref="FileLoadException">Thrown when the wrap mode is "BORDER", which is not supported.</exception>
    /// <exception cref="FileLoadException">Thrown when the texture file for an image ID is not found.</exception>
    /// <returns>A dictionary mapping texture filenames to their corresponding wrap modes (WrapS and WrapT).</returns>
    public static Dictionary<string, (TextureWrap WrapS, TextureWrap WrapT)>
        LoadWrapData(string filename)
    {
        XDocument doc = XDocument.Load(filename);
        XNamespace ns = doc.Root!.Name.Namespace;

        var result = new Dictionary<string, (TextureWrap, TextureWrap)>();

        // image id -> filename
        Dictionary<string, string> images =
            doc.Descendants(ns + "image")
                .Where(i => i.Attribute("id") != null)
                .ToDictionary(
                    i => i.Attribute("id")!.Value,
                    i => Path.GetFileName(
                        i.Element(ns + "init_from")?.Value ?? ""));

        foreach (var effect in doc.Descendants(ns + "effect"))
        {
            // sid -> element
            var newParams = effect
                .Descendants(ns + "newparam")
                .ToDictionary(
                    p => (string)p.Attribute("sid")!,
                    p => p);

            foreach (var samplerParam in newParams.Values)
            {
                var sampler = samplerParam.Element(ns + "sampler2D");
                if (sampler == null)
                    continue;

                string? surfaceSid =
                    sampler.Element(ns + "source")?.Value;

                if (surfaceSid == null ||
                    !newParams.TryGetValue(surfaceSid, out var surfaceParam))
                    continue;

                string? imageId =
                    surfaceParam
                        .Element(ns + "surface")
                        ?.Element(ns + "init_from")
                        ?.Value;

                if (imageId == null ||
                    !images.TryGetValue(imageId, out string? textureFile))
                    continue;

                if (textureFile == null) throw new FileLoadException("Texture file not found for image ID: " + imageId);
                result[textureFile] =
                (
                    ParseWrapMode(
                        sampler.Element(ns + "wrap_s")?.Value),
                    ParseWrapMode(
                        sampler.Element(ns + "wrap_t")?.Value)
                );
            }
        }

        return result;
    }

    /// <summary>
    /// Parses a string representing a texture wrap mode and returns the corresponding TextureWrap enum value. If the input is null or unrecognized, it defaults to TextureWrap.Repeat.
    /// </summary>
    /// <param name="value">The string representation of the wrap mode (e.g., "WRAP", "MIRROR", "CLAMP", "BORDER").</param>
    /// <exception cref="FileLoadException">Thrown when the wrap mode is "BORDER", which is not supported.</exception>
    /// <returns>The corresponding TextureWrap enum value.</returns>
    private static TextureWrap ParseWrapMode(string? value)
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
}