using System;
using System.Numerics;
using System.Collections.Generic;
using odl3d.Loaders;

namespace odl3d;

/// <summary>
/// Represents a single part of the 3D model, with its own mesh, texture, and local transformation relative to the parent model.
/// </summary>
public class ModelPart : Object3D
{
    /// <summary>
    /// The parent model to which this part belongs. The parent model provides the overall transformation and context for this part, allowing it to be positioned and rendered correctly within the scene.
    /// </summary>
    private readonly Model Parent;

    /// <summary>
    /// The local transformation matrix for this model part, defining its position, rotation, and scale relative to the parent model. This matrix is combined with the parent's model matrix to compute the final transformation for rendering this part in the scene.
    /// </summary>
    private readonly Matrix4x4 LocalTransform;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelPart"/> class, representing a single part of a 3D model. The constructor takes the parent model, scene, mesh, texture, and local transformation matrix as parameters, and initializes the corresponding properties. The model part is not automatically added to the scene; it is managed by the parent model.
    /// </summary>
    /// <param name="parent">The parent model to which this part belongs.</param>
    /// <param name="scene">The scene to which this part belongs.</param>
    /// <param name="mesh">The mesh for this part.</param>
    /// <param name="texture">The texture for this part.</param>
    /// <param name="localTransform">The local transformation matrix for this part.</param>
    public ModelPart(Model parent, Scene<Object3D> scene, Mesh mesh, Texture? texture, Matrix4x4 localTransform) : base(scene, mesh, texture, addToScene: false)
    {
        Parent = parent;
        LocalTransform = localTransform;
    }

    /// <summary>
    /// Gets the model matrix for this model part, which is the product of its local transformation matrix and the parent's model matrix. This matrix is used to transform the part's vertices from local space to world space for rendering.
    /// </summary>
    /// <returns>The model matrix for this model part.</returns>
    public override Matrix4x4 GetModelMatrix() => LocalTransform * Parent.GetModelMatrix();
}

/// <summary>
/// Represents a 3D model composed of multiple sub-objects, each with its own mesh and texture. Provides methods for loading models from DAE and OBJ files, and handles drawing and disposal of its sub-objects.
/// </summary>
public class Model : Object3D
{
    /// <summary>
    /// The list of sub-objects that make up this 3D model. Each sub-object has its own mesh and texture.
    /// </summary>
    protected List<Object3D> Objects = new List<Object3D>();

    /// <summary>
    /// The total number of vertices rendered by this model's mesh parts.
    /// </summary>
    public override int VertexCount
    {
        get
        {
            int vertexCount = 0;
            foreach (Object3D obj in Objects)
            {
                if (obj.Visible) vertexCount += obj.VertexCount;
            }
            return vertexCount;
        }
    }

    /// <summary>
    /// Initializes a new instance of the Model class with the specified scene, meshes, and textures.
    /// </summary>
    /// <param name="scene">The scene to which this model belongs.</param>
    /// <param name="meshes">An array of meshes that make up the model.</param>
    /// <param name="textures">An array of textures corresponding to the meshes.</param>
    public Model(Scene<Object3D> scene, Mesh[] meshes, Texture?[] textures, Matrix4x4[]? localTransforms = null) : base(scene)
    {
        for (int i = 0; i < meshes.Length; i++)
        {
            Matrix4x4 localTransform = localTransforms != null && i < localTransforms.Length
                ? localTransforms[i]
                : Matrix4x4.Identity;
            Object3D obj = new ModelPart(this, scene, meshes[i], textures[i], localTransform);
            Objects.Add(obj);
        }
    }

    /// <summary>
    /// Loads a 3D model from a DAE (Collada) file and returns a new Model instance.
    /// </summary>
    /// <param name="scene">The scene to which the loaded model will belong.</param>
    /// <param name="filename">The path to the DAE file to load.</param>
    /// <returns>A new Model instance representing the loaded 3D model.</returns>
    /// <exception cref="ArgumentException">Thrown if the filename is invalid or the DAE file cannot be loaded.</exception>
    public static Model LoadDAE(Scene<Object3D> scene, string filename)
    {
        string? daeFolder = System.IO.Path.GetDirectoryName(filename);
        if (daeFolder == null) throw new ArgumentException("Invalid filename: " + filename);
        (Mesh[] meshes, Texture?[] textures, Matrix4x4[] localTransforms) = DaeLoader.Load(filename, daeFolder);
        return new Model(scene, meshes, textures, localTransforms);
    }

    /// <summary>
    /// Loads a 3D model from an OBJ file and returns a new Model instance.
    /// </summary>
    /// <param name="scene">The scene to which the loaded model will belong.</param>
    /// <param name="objFilename">The path to the OBJ file to load.</param>
    /// <returns>A new Model instance representing the loaded 3D model.</returns>
    /// <exception cref="ArgumentException">Thrown if the filename is invalid or the OBJ file cannot be loaded.</exception>
    public static Model LoadOBJ(Scene<Object3D> scene, string objFilename)
    {
        string? objFolder = System.IO.Path.GetDirectoryName(objFilename);
        if (objFolder == null) throw new ArgumentException("Invalid filename: " + objFilename);
        ObjFile objFile = ObjLoader.Load(objFilename);
        Dictionary<string, Material> materials = new Dictionary<string, Material>();
        if (objFile.MtlFilename != null)
        {
            materials = MtlLoader.Load(objFolder + "/" + objFile.MtlFilename);
        }
        Mesh[] meshes = new Mesh[objFile.Meshes.Count];
        Texture?[] textures = new Texture?[objFile.Meshes.Count];
        int idx = 0;
        foreach (var kvp in objFile.Meshes)
        {
            string objectName = kvp.Key;
            ObjGroup objGroup = kvp.Value;
            Mesh groupMesh = objGroup.Mesh;
            string? mtlName = objGroup.MtlName;
            Texture? tex = null;
            if (mtlName != null && materials.ContainsKey(mtlName))
            {
                Material mat = materials[mtlName];
                tex = new Texture(objFolder + "/" + mat.Name + ".png");
            }
            meshes[idx] = groupMesh;
            textures[idx] = tex;
            idx++;
        }
        return new Model(scene, meshes, textures);
    }

    /// <summary>
    /// Draws the model using the specified shader and view-projection matrix. Sub-objects are filtered by the given render pass so opaque and transparent parts can be drawn in separate passes across the whole scene.
    /// </summary>
    /// <param name="shader">The shader to use for rendering the model.</param>
    /// <param name="viewProjection">The combined view-projection matrix for the current camera.</param>
    /// <param name="pass">Which render pass is currently being drawn; sub-objects not belonging to this pass are skipped.</param>
    public override void Draw(ShaderProgram shader, Matrix4x4 viewProjection, RenderPass pass = RenderPass.Opaque)
    {
        foreach (var obj in Objects)
        {
            obj.Visible = Visible;
            if (Texture != null) obj.Texture = Texture;
            obj.Color = Color;
            obj.TextureColor = TextureColor;
            obj.Draw(shader, viewProjection, pass);
        }
    }

    /// <summary>
    /// Disposes of the model and releases all associated resources, including its constituent objects.
    /// </summary>
    public override void Dispose()
    {
        if (Disposed) return;
        base.Dispose();
        foreach (var obj in Objects)
        {
            obj.Dispose();
        }
        Objects.Clear();
    }
}