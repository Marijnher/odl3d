using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// A drawable 3D object consisting of a Mesh and an optional Texture, positioned and scaled in world space. The object can be drawn using a Shader and a view-projection matrix. Implements IDisposable to release GPU resources when no longer needed.
/// </summary>
public class Object : IDisposable
{
    /// <summary>
    /// The Scene3D instance to which this object belongs. The scene provides context for the object's position and scale in world space, as well as access to the camera and other scene properties.
    /// </summary>
    protected Scene<Object> Scene;

    /// <summary>
    /// The texture to use when drawing this object, or null to draw without a texture.
    /// </summary>
    public Texture? Texture;

    /// <summary>
    /// The color multiplier applied to the texture when drawing this object. Defaults to white, which means the texture is drawn with its original colors. Changing this color can tint the texture.
    /// </summary>
    public Color TextureColor = Color.White;

    /// <summary>
    /// The solid color used when this object has no texture. Defaults to gray.
    /// </summary>
    public Color Color = Color.Gray;

    /// <summary>
    /// The mesh to use when drawing this object.
    /// </summary>
    public Mesh Mesh;

    /// <summary>
    /// The position of this object in world space.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The scale of this object in world space; defaults to (1,1,1).
    /// </summary>
    public Vector3 Scale = Vector3.One;
    
    /// <summary>
    /// If true, the texture will be automatically disposed when this object is disposed. If false, the texture will not be disposed and must be managed externally. Defaults to true.
    /// </summary>
    public bool AutoDisposeTexture = true;

    /// <summary>
    /// Indicates whether this object has been disposed and its resources released. After disposing, the object should not be used again.
    /// </summary>
    public bool Disposed { get; private set; } = false;

    /// <summary>
    /// Creates a new Object with the given mesh and optional texture.
    /// </summary>
    /// <param name="scene">The scene to which this object belongs.</param>
    /// <param name="mesh">The mesh to use when drawing this object.</param>
    /// <param name="texture">The texture to use when drawing this object, or null to draw without a texture.</param>
    public Object(Scene<Object> scene, Mesh mesh, Texture? texture = null) 
    {
        this.Mesh = mesh;
        this.Texture = texture;
        this.Scene = scene;
        scene.Add(this);
    }

    /// <summary>
    /// Creates a new Object by loading a mesh from the given OBJ file and using the optional texture. The mesh is loaded using the ObjLoader class.
    /// </summary>
    /// <param name="scene">The scene to which this object belongs.</param>
    /// <param name="meshFilename">The file path to the Wavefront .obj file to be loaded as the mesh for this object.</param>
    /// <param name="texture">The texture to use when drawing this object, or null to draw without a texture.</param>
    public Object(Scene<Object> scene, string meshFilename, Texture? texture = null) :
        this(scene, ObjLoader.Load(meshFilename), texture) { }


    ~Object()
    {
        if (!Disposed) Console.WriteLine("Warning: Object was not disposed before being finalized. This may cause a GL resource leak.");
    }

    /// <summary>
    /// Returns the model matrix for this object, which transforms from local space to world space.
    /// </summary>
    /// <returns></returns>
    public virtual Matrix4x4 GetModelMatrix() =>
        Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Position + Scene.SceneOffset);

    /// <summary>Draws this object using a precomputed view*projection matrix.</summary>
    public virtual void Draw(Shader shader, Matrix4x4 viewProjection)
    {
        Matrix4x4 mvp = GetModelMatrix() * viewProjection;

        shader.Use();
        shader.SetMatrix4("uMVP", mvp);
        shader.SetInt("uTexture", 0);
        shader.SetInt("uUseTexture", Texture != null ? 1 : 0);
        shader.SetColor("uColor", Color);
        shader.SetColor("texColor", TextureColor);
        if (Texture != null)
        {
            Texture.Bind(0);
        }
        else
        {
            GL.glBindTexture(GL.GL_TEXTURE_2D, 0);
        }
        Mesh.Draw();
    }

    public void Dispose()
    {
        if (Disposed) return;
        Mesh.Dispose();
        if (AutoDisposeTexture)
            Texture?.Dispose();
        Disposed = true;
    }
}
