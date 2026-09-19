using System;
using System.Collections.Generic;
using System.Numerics;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// Identifies which rendering pass is currently active for a Scene3D: opaque geometry is drawn first with normal depth writes, then transparent geometry is drawn with depth writes disabled so it blends correctly against whatever was drawn behind it, regardless of scene/model ordering.
/// </summary>
public enum RenderPass
{
    Opaque,
    Transparent
}

/// <summary>
/// Represents a 3D object in the scene, consisting of a mesh and an optional texture, with properties for position, rotation, scale, and color. The object can be drawn using a shader and a view-projection matrix, and it manages its own GPU resources.
/// </summary>
public class Object3D : Drawable
{
    /// <summary>
    /// The Scene3D instance to which this Fobject belongs. The scene provides context for the object's position and scale in world space, as well as access to the camera and other scene properties.
    /// </summary>
    public Scene<Object3D> Scene;

    protected IRenderDevice Renderer => Window.Renderer;

    /// <summary>
    /// The texture to use when drawing this object, or null to draw without a texture.
    /// </summary>
    public Texture? Texture;

    public Sampler Sampler;

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
    /// The number of vertices rendered by this object, excluding child objects.
    /// </summary>
    public virtual int VertexCount => Mesh?.VertexCount ?? 0;

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
    public Object3D(Scene<Object3D> scene, Mesh? mesh = null, Texture? texture = null) 
    {
        Scene = scene;
        Mesh = mesh;
        Texture = texture;
        Sampler = new Sampler();
        scene.Add(this);
    }

    protected Object3D(Scene<Object3D> scene, Mesh? mesh, Texture? texture, bool addToScene) 
    {
        Scene = scene;
        Mesh = mesh;
        Texture = texture;
        Sampler = new Sampler();
        if (addToScene) scene.Add(this);
    }

    ~Object3D()
    {
        if (!Disposed) Console.WriteLine("Warning: Object was not disposed before being finalized. This may cause a renderer resource leak.");
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
        Matrix4x4.CreateScale(Scale) *
        Matrix4x4.CreateRotationX(MathF.PI / 180 * Rotation.X) *
        Matrix4x4.CreateRotationY(MathF.PI / 180 * Rotation.Y) *
        Matrix4x4.CreateRotationZ(MathF.PI / 180 * Rotation.Z) *
        Matrix4x4.CreateTranslation(Position + Scene.Position);

    public virtual ObjectShaderData GetShaderData()
    {
        var shaderData = new ObjectShaderData
        {
            Model = GetModelMatrix(),
            UseTexture = (uint) (Texture != null && !Texture.Disposed ? 1 : 0),
            TexColor = TextureColor.ToVector4(),
            ObjColor = Color.ToVector4()
        };
        return shaderData;
    }
    
    /// <summary>
    /// True if this object must be alpha-blended against whatever has already been drawn behind it (e.g. a soft shadow decal), as opposed to being fully opaque or a hard 0/255 alpha cutout. Transparent objects are rendered in a second pass, after all opaque objects, without writing to the depth buffer, so they blend correctly regardless of scene/model ordering.
    /// </summary>
    public virtual bool IsTransparent => (Texture != null && Texture.HasPartialAlpha) || (Texture == null && Color.A != 0 && Color.A != 255);

    /// <summary>
    /// Draws this object using the specified shader and the given view-projection matrix. The view-projection matrix is typically obtained from the camera and represents the combined view and projection transformations. This method sets up the necessary shader uniforms, binds the texture if available, and then draws the mesh associated with this object. Only objects matching the requested render pass (opaque or transparent) are drawn; the other pass is skipped so callers can render opaque geometry before transparent geometry.
    /// </summary>
    /// <param name="shader">The shader program to use for rendering this object.</param>
    /// <param name="viewProjection">The combined view and projection matrix, typically obtained from the camera.</param>
    /// <param name="pass">Which render pass is currently being drawn; the object is skipped if it does not belong to this pass.</param>
    public virtual void Draw(IRenderPass pass, RenderPass passType = RenderPass.Opaque)
    {
        if (!Visible || Disposed || Mesh == null || Mesh.Disposed) return;
        if (passType == RenderPass.Transparent != IsTransparent) return;

        ShaderPipeline pipeline = Mesh.HasNormals ? Scene.Window.PipelineWithNormals : Scene.Window.PipelineNoNormals;
        pass.SetRenderPipeline(pipeline.Pipeline);

        pass.SetVertexBuffer(Mesh.Vertices);
        pass.SetIndexBuffer(Mesh.Indices);
        if (Texture != null && !Texture.Disposed)
        {
            if (!Texture.Uploaded) Texture.Upload();
            pass.SetTexture(Texture.RenderTexture);
        }
        pass.SetSampler(Sampler.RenderSampler);
        pass.DrawIndexed();

        // shader.Use();
        // shader.SetMatrix("uMVP", model * viewProjection);
        // shader.SetInt("uTexture", 0);
        // shader.SetInt("uUseTexture", Texture == null ? 0 : 1);
        // shader.SetColor("uColor", Color);
        // shader.SetColor("texColor", TextureColor);
        // Lighting uniforms are only meaningful for meshes that carry normals; shaders without them ignore these.
        // shader.SetInt("uLit", Mesh.HasNormals ? 1 : 0);
        // if (Mesh.HasNormals) shader.SetMatrix("uModel", model);

        // Renderer.BindTexture(Texture);

        // Mesh.Draw();
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

