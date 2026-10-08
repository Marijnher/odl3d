using System;
using System.Collections.Generic;
using System.Numerics;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// Represents a 3D object in the scene, consisting of a mesh and an optional texture, with properties for position, rotation, scale, and color. The object can be drawn using a shader and a view-projection matrix, and it manages its own GPU resources.
/// </summary>
public class Object3D : Drawable
{
    /// <summary>
    /// The Scene3D instance to which this Fobject belongs. The scene provides context for the object's position and scale in world space, as well as access to the camera and other scene properties.
    /// </summary>
    public Scene<Object3D>? Scene { get; private set; }

    /// <summary>
    /// The renderer instance used to draw this object. This is obtained from the window associated with the scene.
    /// </summary>
    protected IRenderDevice Renderer => (Scene ?? throw new InvalidOperationException("Attach the object to a scene before drawing it.")).Window.Renderer;

    /// <summary>
    /// The texture to use when drawing this object, or null to draw without a texture.
    /// </summary>
    public Texture? Texture;

    /// <summary>
    /// The sampler to use when drawing this object. This controls how the texture is sampled, including filtering and wrapping modes.
    /// </summary>
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
    /// Creates a new Object3D with the given mesh and optional texture.
    /// </summary>
    /// <param name="scene">The scene to which this object belongs.</param>
    /// <param name="mesh">The mesh to use when drawing this object, or null to draw nothing.</param>
    /// <param name="texture">The texture to use when drawing this object, or null to draw without a texture.</param>
    public Object3D(Mesh? mesh = null, Texture? texture = null)
    {
        Mesh = mesh;
        Texture = texture;
        Sampler = new Sampler();
    }

    /// <summary>
    /// Attaches this object to the specified scene. This method should be called when adding the object to a scene.
    /// </summary>
    /// <param name="scene">The scene to attach this object to.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    internal virtual void Attach(Scene<Object3D> scene)
    {
        if (Disposed) throw new ObjectDisposedException(nameof(Object3D));
        if (Scene != null) throw new InvalidOperationException("The object already belongs to a scene.");
        Scene = scene;
    }

    /// <summary>
    /// Detaches this object from the specified scene. This method should be called when removing the object from a scene.
    /// </summary>
    /// <param name="scene">The scene to detach this object from.</param>
    internal virtual void Detach(Scene<Object3D>? scene)
    {
        if (ReferenceEquals(Scene, scene)) Scene = null;
    }

    ~Object3D()
    {
        if (!Disposed) Console.WriteLine("Warning: Object was not disposed before being finalized. This may cause a renderer resource leak.");
    }

    /// <summary>
    /// Enables input handling for this object. If input handling is already enabled, an InvalidOperationException will be thrown.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if input handling is already enabled for this object or if the object is not attached to a scene.</exception>
    public override void EnableInput()
    {
        if (InputManager != null)
            throw new InvalidOperationException("Input handling is already enabled for this object.");
        if (Scene == null)
            throw new InvalidOperationException("Attach the object to a scene before enabling input.");
        InputManager = new ProxyInputManager(Scene.Window);
    }

    /// <summary>
    /// Disables input handling for this object. If input handling is not currently enabled, an InvalidOperationException will be thrown.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if input handling is not currently enabled for this object.</exception>
    public override void DisableInput()
    {
        if (InputManager == null)
            throw new InvalidOperationException("Input handling is not enabled for this object.");
        InputManager.Dispose();
        InputManager = null;
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
        Matrix4x4.CreateTranslation(Position + (Scene?.Position ?? Vector3.Zero));

    /// <summary>
    /// Returns the shader data for this object, which includes the model matrix, texture usage, colors, and normal information.
    /// </summary>
    /// <returns>An ObjectShaderData instance containing the relevant shader information for this object.</returns>
    public virtual ObjectShaderData GetShaderData()
    {
        var shaderData = new ObjectShaderData
        {
            Model = GetModelMatrix(),
            UseTexture = Texture != null && !Texture.Disposed,
            TexColor = TextureColor.ToVector4(),
            ObjColor = Color.ToVector4(),
            HasNormals = Mesh?.HasNormals ?? false
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

        Scene<Object3D> scene = Scene ?? throw new InvalidOperationException("Attach the object to a scene before drawing it.");
        pass.SetRenderPipeline(scene.Window.ShaderPipeline.Pipeline);

        Mesh.EnsureUploaded(Renderer);
        pass.SetVertexBuffer(Mesh.Vertices);
        pass.SetIndexBuffer(Mesh.Indices);
        if (Texture != null && !Texture.Disposed)
        {
            Texture.EnsureUploaded(Renderer);
            pass.SetTexture(Texture.RenderTexture);
        }
        Sampler.EnsureCreated(Renderer);
        pass.SetSampler(Sampler.RenderSampler);
        pass.DrawIndexed();
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

