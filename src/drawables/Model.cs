using System;
using System.Numerics;
using System.Collections.Generic;
using odl3d.Loaders;
using odl3d.Renderer;
using System.Linq;

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
    public ModelPart(Model parent, Mesh mesh, Texture? texture, Sampler? sampler, Matrix4x4 localTransform) : base(mesh, texture)
    {
        Parent = parent;
        Sampler = sampler ?? new Sampler();
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
    /// The array of object shader data for all sub-objects in the model. This array is used to store the latest shader data for each sub-object before it is uploaded to the GPU buffer.
    /// </summary>
    protected ObjectShaderData[] ObjectShaderDataArray;

    /// <summary>
    /// The GPU buffer that stores the object shader data for all sub-objects in the model. This buffer is updated with the latest data from the ObjectShaderDataArray before rendering.
    /// </summary>
    protected IBuffer<ObjectShaderData>? ObjectShaderDataBuffer;

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

    public override BoundingBox3D GetWorldBounds()
    {
        BoundingBox3D bounds = BoundingBox3D.Empty;
        foreach (Object3D part in Objects)
            bounds = bounds.Union(part.GetWorldBounds());
        return bounds;
    }

    /// <summary>
    /// Per-part lighting properties from the model file, index-aligned with the parts. Null when the file provided none.
    /// </summary>
    private readonly SourceMaterial?[]? _sourceMaterials;

    private readonly SourceLight[] _sourceLights;

    private readonly List<(SourceLight Source, Light Light)> _sceneLights = new();

    /// <summary>
    /// When true, <see cref="Emissive"/>, <see cref="Object3D.Specular"/> and <see cref="Object3D.Shininess"/> replace the material properties read from the model file. By default the file's values are used and <see cref="Emissive"/> is added on top of them.
    /// </summary>
    public bool OverrideSourceMaterials;

    /// <summary>
    /// The lights that were created from light definitions in the model file while the model belongs to a <see cref="Scene3D"/>. They follow the model's transform.
    /// </summary>
    public IEnumerable<Light> SourceLights => _sceneLights.Select(entry => entry.Light);

    internal override bool HasTransparentContent => IsTransparent || Objects.Any(obj => obj.IsTransparent);

    /// <summary>
    /// Initializes a new instance of the Model class with the specified scene, meshes, and textures.
    /// </summary>
    /// <param name="scene">The scene to which this model belongs.</param>
    /// <param name="meshes">An array of meshes that make up the model.</param>
    /// <param name="textures">An array of textures corresponding to the meshes.</param>
    /// <param name="materials">Optional per-mesh lighting properties read from the model file.</param>
    /// <param name="lights">Optional lights defined in the model file. They are added to the scene together with the model.</param>
    public Model(Mesh[] meshes, Texture?[] textures, Sampler?[]? samplers = null, Matrix4x4[]? localTransforms = null, SourceMaterial?[]? materials = null, IEnumerable<SourceLight>? lights = null) : base()
    {
        _sourceMaterials = materials;
        _sourceLights = lights?.ToArray() ?? [];
        for (int i = 0; i < meshes.Length; i++)
        {
            Matrix4x4 localTransform = localTransforms != null && i < localTransforms.Length
                ? localTransforms[i]
                : Matrix4x4.Identity;
            Object3D obj = new ModelPart(this, meshes[i], textures[i], samplers?[i], localTransform);
            Objects.Add(obj);
        }
        ObjectShaderDataArray = new ObjectShaderData[meshes.Length];
    }

    internal override void Attach(Scene<Object3D> scene)
    {
        base.Attach(scene);
        foreach (Object3D part in Objects)
            part.Attach(scene);
        if (scene is Scene3D scene3D)
        {
            foreach (SourceLight source in _sourceLights)
            {
                Light light = source.CreateLight(GetModelMatrix(), scene3D.Position);
                scene3D.AddLight(light);
                _sceneLights.Add((source, light));
            }
        }
    }

    private void RemoveSceneLights()
    {
        foreach ((_, Light light) in _sceneLights)
        {
            foreach (Scene3D scene in light.Scenes.ToArray())
                scene.RemoveLight(light);
        }
        _sceneLights.Clear();
    }

    internal override void Detach(Scene<Object3D>? scene)
    {
        RemoveSceneLights();
        foreach (Object3D part in Objects)
            part.Detach(scene);
        base.Detach(scene);
    }

    /// <summary>
    /// Loads a 3D model from a DAE (Collada) file and returns a new Model instance.
    /// </summary>
    /// <param name="scene">The scene to which the loaded model will belong.</param>
    /// <param name="filename">The path to the DAE file to load.</param>
    /// <returns>A new Model instance representing the loaded 3D model.</returns>
    /// <exception cref="ArgumentException">Thrown if the filename is invalid or the DAE file cannot be loaded.</exception>
    public static Model LoadDAE(string filename)
    {
        string? daeFolder = System.IO.Path.GetDirectoryName(filename);
        if (daeFolder == null) throw new ArgumentException("Invalid filename: " + filename);
        DaeLoader.DaeModelData data = DaeLoader.LoadModelData(filename, daeFolder);
        return new Model(data.Meshes, data.Textures, data.Samplers, data.LocalTransforms, data.Materials, data.Lights);
    }

    /// <summary>
    /// Loads a 3D model from an OBJ file and returns a new Model instance.
    /// </summary>
    /// <param name="scene">The scene to which the loaded model will belong.</param>
    /// <param name="objFilename">The path to the OBJ file to load.</param>
    /// <returns>A new Model instance representing the loaded 3D model.</returns>
    /// <exception cref="ArgumentException">Thrown if the filename is invalid or the OBJ file cannot be loaded.</exception>
    public static Model LoadOBJ(string objFilename)
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
        SourceMaterial?[] sourceMaterials = new SourceMaterial?[objFile.Meshes.Count];
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
                sourceMaterials[idx] = new SourceMaterial(
                    SourceMaterial.ToColor(mat.Ke[0], mat.Ke[1], mat.Ke[2]),
                    SourceMaterial.ToColor(mat.Ks[0], mat.Ks[1], mat.Ks[2]),
                    MathF.Max(mat.Ns, 1f));
            }
            meshes[idx] = groupMesh;
            textures[idx] = tex;
            idx++;
        }
        return new Model(meshes, textures, materials: sourceMaterials);
    }

    /// <summary>
    /// Binds the object shader data to the GPU buffer, updating it with the latest data from all objects in the model.
    /// </summary>
    public void UpdateObjectShaderData()
    {
        if (ObjectShaderDataBuffer == null)
        {
            ObjectShaderDataBuffer = Renderer.CreateBuffer<ObjectShaderData>(new BufferDescription
            {
                Size = Objects.Count,
                Usage = BufferUsage.Uniform
            });
        }
        for (int i = 0; i < Objects.Count; i++)
        {
            ObjectShaderDataArray[i] = Objects[i].GetShaderData();
        }
        ObjectShaderDataBuffer.SetData(ObjectShaderDataArray, 0, Objects.Count);
    }

    /// <summary>
    /// Draws the model using the specified shader and view-projection matrix. Sub-objects are filtered by the given render pass so opaque and transparent parts can be drawn in separate passes across the whole scene.
    /// </summary>
    /// <param name="shader">The shader to use for rendering the model.</param>
    /// <param name="viewProjection">The combined view-projection matrix for the current camera.</param>
    /// <param name="pass">Which render pass is currently being drawn; sub-objects not belonging to this pass are skipped.</param>
    public unsafe override void Draw(IRenderPass pass, RenderPass passType = RenderPass.Opaque)
    {
        for (int i = 0; i < Objects.Count; i++)
        {
            var obj = Objects[i];
            obj.Visible = Visible;
            if (Texture != null) obj.Texture = Texture;
            obj.Color = Color;
            obj.TextureColor = TextureColor;
            obj.Lit = Lit;
            SourceMaterial? source = OverrideSourceMaterials ? null : _sourceMaterials?[i];
            if (source == null)
            {
                obj.Emissive = Emissive;
                obj.Specular = Specular;
                obj.Shininess = Shininess;
            }
            else
            {
                obj.Emissive = AddColors(source.Emissive, Emissive);
                obj.Specular = source.Specular;
                obj.Shininess = source.Shininess;
                if (source.Diffuse is Color diffuse && obj.Texture == null)
                    obj.Color = MultiplyColors(Color, diffuse);
            }
        }
        UpdateObjectShaderData();
        BoundingFrustum3D? frustum = Scene is Scene3D scene3D ? scene3D.CreateFrustum() : null;
        for (int i = 0; i < Objects.Count; i++)
        {
            var obj = Objects[i];
            if (frustum is BoundingFrustum3D modelFrustum && !modelFrustum.Intersects(obj.GetWorldBounds()))
                continue;
            pass.SetUniformBuffer(ObjectShaderDataBuffer!, 0, (uint) (i * sizeof(ObjectShaderData)));
            obj.Draw(pass, passType);
        }
    }

    private static Color MultiplyColors(Color a, Color b) =>
        new((byte) (a.R * b.R / 255), (byte) (a.G * b.G / 255), (byte) (a.B * b.B / 255), a.A);

    private static Color AddColors(Color a, Color b) =>
        new((byte) Math.Min(a.R + b.R, 255), (byte) Math.Min(a.G + b.G, 255), (byte) Math.Min(a.B + b.B, 255));

    /// <summary>
    /// Updates the model and all its constituent objects based on the elapsed time.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update, in seconds.</param>
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (_sceneLights.Count > 0 && Scene is Scene3D scene3D)
        {
            Matrix4x4 model = GetModelMatrix();
            foreach ((SourceLight source, Light light) in _sceneLights)
                source.Update(light, model, scene3D.Position);
        }
        Objects.ForEach(obj => obj.Update(deltaTime));
    }

    /// <summary>
    /// Disposes of the model and releases all associated resources, including its constituent objects.
    /// </summary>
    public override void Dispose()
    {
        if (Disposed) return;
        RemoveSceneLights();
        base.Dispose();
        ObjectShaderDataBuffer?.Dispose();
        foreach (var obj in Objects)
        {
            obj.Dispose();
        }
        Objects.Clear();
    }
}