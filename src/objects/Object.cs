using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// A drawable 3D object consisting of a Mesh and an optional Texture, positioned and scaled in world space. The object can be drawn using a Shader and a view-projection matrix. Implements IDisposable to release GPU resources when no longer needed.
/// </summary>
public class Object : Drawable
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
    public Mesh? Mesh;

    /// <summary>
    /// The scale of this object in world space; defaults to (1,1,1).
    /// </summary>
    public Vector3 Scale = Vector3.One;
    
    /// <summary>
    /// If true, the texture will be automatically disposed when this object is disposed. If false, the texture will not be disposed and must be managed externally. Defaults to true.
    /// </summary>
    public bool AutoDisposeTexture = true;

    /// <summary>
    /// If true, the mesh will be automatically disposed when this object is disposed. If false, the mesh will not be disposed and must be managed externally. Defaults to true.
    /// </summary>
    public bool AutoDisposeMesh = true;

    /// <summary>
    /// Creates a new Object with the given mesh and optional texture.
    /// </summary>
    /// <param name="scene">The scene to which this object belongs.</param>
    /// <param name="mesh">The mesh to use when drawing this object, or null to draw nothing.</param>
    /// <param name="texture">The texture to use when drawing this object, or null to draw without a texture.</param>
    public Object(Scene<Object> scene, Mesh? mesh = null, Texture? texture = null) 
    {
        this.Scene = scene;
        this.Mesh = mesh;
        this.Texture = texture;
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
    /// Enables or disables input handling for this object. When enabled, the object will create a ProxyInputManager to handle input events. When disabled, the ProxyInputManager will be disposed and input events will no longer be processed for this object. This method allows the user to control whether the object should respond to user input.
    /// </summary>
    /// <param name="enable">True to enable input handling; false to disable it.</param>
    public override void SetEnableInput(bool enable)
    {
        if (enable && InputManager == null)
        {
            InputManager = new ProxyInputManager(Scene.Window);
        }
        else if (!enable && InputManager != null)
        {
            InputManager.Dispose();
            InputManager = null;
        }
    }

    /// <summary>
    /// Returns the model matrix for this object, which transforms from local space to world space.
    /// </summary>
    /// <returns>The model matrix that transforms this object's local coordinates to world coordinates.</returns>
    public virtual Matrix4x4 GetModelMatrix() =>
        Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Position + Scene.Position);

    /// <summary>
    /// Draws this object using the specified shader and the given view-projection matrix. The view-projection matrix is typically obtained from the camera and represents the combined view and projection transformations. This method sets up the necessary shader uniforms, binds the texture if available, and then draws the mesh associated with this object.
    /// </summary>
    /// <param name="shader">The shader program to use for rendering this object.</param>
    /// <param name="viewProjection">The combined view and projection matrix, typically obtained from the camera.</param>
    public virtual void Draw(Shader shader, Matrix4x4 viewProjection)
    {
        if (!Visible || Disposed || Mesh == null) return;
        Matrix4x4 mvp = GetModelMatrix() * viewProjection;

        shader.Use();
        shader.SetMatrix4("uMVP", mvp);
        shader.SetInt("uTexture", 0);
        shader.SetInt("uUseTexture", Texture != null ? 1 : 0);
        shader.SetColor("uColor", Color);
        shader.SetColor("texColor", TextureColor);
        if (Texture != null) Texture.Bind(0);
        else GL.glBindTexture(GL.GL_TEXTURE_2D, 0);
        Mesh.Draw();
    }

    /// <summary>
    /// Updates the state of this object. This method should be called once per frame to ensure that the object's state is updated correctly.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
    }

    /// <summary>
    /// Disposes of the resources used by this object, including its mesh and optionally its texture. After calling this method, the object should not be used again.
    /// </summary>
    public override void Dispose()
    {
        if (Disposed) return;
        if (AutoDisposeMesh)
            Mesh?.Dispose();
        if (AutoDisposeTexture)
            Texture?.Dispose();
        base.Dispose();
        Scene?.Remove(this);
        Disposed = true;
    }
}
