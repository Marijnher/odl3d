using System;
using System.Collections.Generic;
using System.Numerics;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// A 3D scene containing a collection of Object instances and a Camera. The scene provides methods to add and remove objects, and an implementation of the Draw method that renders all objects in the scene using the camera's view and projection matrices.
/// </summary>
public class Scene3D : Scene<Object3D>
{
    /// <summary>
    /// The camera used to render the scene. The camera's view and projection matrices are combined to create the view-projection matrix used for rendering the objects in the scene. The camera can be configured with position, orientation, field of view, aspect ratio, and other properties to control how the scene is viewed.
    /// </summary>
    /// <param name="window">The window associated with the scene, used to determine the rendering context and other properties.</param>
    public Scene3D(Window window) : base(window)
    {
        _readOnlyLights = _lights.AsReadOnly();
        _lighting = new SceneLighting(Renderer);
        Window.AddScene(this);
    }

    private readonly List<Light> _lights = new();
    private readonly IReadOnlyList<Light> _readOnlyLights;
    private readonly SceneLighting _lighting;

    /// <summary>
    /// The lights explicitly added to this scene with <see cref="AddLight"/>.
    /// </summary>
    public IReadOnlyList<Light> Lights => _readOnlyLights;

    /// <summary>
    /// The light applied to every lit surface regardless of the scene's lights, so surfaces facing away from all lights are not pitch black. Only used while the scene has at least one enabled light.
    /// </summary>
    public Color AmbientColor = new(40, 40, 40);

    /// <summary>
    /// True when at least one light is enabled. Scenes without active lights render every object at its flat, unlit color; as soon as one light is active, lit objects (<see cref="Object3D.Lit"/>) are shaded.
    /// </summary>
    public bool HasActiveLights
    {
        get
        {
            foreach (Light light in _lights)
                if (light.Enabled) return true;
            return false;
        }
    }

    /// <summary>
    /// Adds a light to the scene. The same light can be added to several scenes, so it can be created once and shared. Adding a light that is already in this scene does nothing.
    /// </summary>
    /// <param name="light">The light to add.</param>
    public void AddLight(Light light)
    {
        ArgumentNullException.ThrowIfNull(light);
        ObjectDisposedException.ThrowIf(Disposed, this);
        if (_lights.Contains(light)) return;
        light.AttachTo(this);
        _lights.Add(light);
    }

    /// <summary>
    /// Removes a light from the scene. Nothing happens if the light is not part of this scene.
    /// </summary>
    /// <param name="light">The light to remove.</param>
    public void RemoveLight(Light light)
    {
        if (_lights.Remove(light)) light.DetachFrom(this);
    }

    /// <summary>
    /// Uploads and binds the scene's view/projection data and, when the scene has active lights, its lighting data.
    /// </summary>
    private void BindSceneData(IRenderPass pass)
    {
        UpdateSceneShaderData(pass);
        if (!HasActiveLights) return;
        Vector4 ambient = AmbientColor.ToVector4();
        _lighting.Bind(pass, _lights, new Vector3(ambient.X, ambient.Y, ambient.Z), Camera.Position, Position);
    }

    /// <summary>
    /// Draws all objects in the scene using the given shader. Runs both the opaque and transparent pass back to back for this scene alone; prefer DrawPass via Window.Render when multiple Scene3D instances share a window, so transparency blends correctly against every scene's opaque geometry rather than just this scene's.
    /// </summary>
    /// <param name="shader">The shader program to use for rendering the scene.</param>
    /// <param name="renderPass">The render pass to use for rendering the scene.</param>
    public unsafe override void Draw(IRenderPass pass, RenderPass passType = RenderPass.Opaque)
    {
        if (!Visible || Disposed) return;

        if (passType == RenderPass.Transparent)
        {
            List<(int Index, float Depth)> transparentObjects = new(GetTransparentObjects());
            transparentObjects.Sort((left, right) => right.Depth.CompareTo(left.Depth));
            foreach ((int index, _) in transparentObjects)
                DrawObject(pass, index, passType);
            return;
        }

        BoundingFrustum3D frustum = CreateFrustum();
        BindSceneData(pass);
        for (int i = 0; i < Objects.Count; i++)
        {
            Object3D obj = Objects[i];
            if (!obj.Visible || obj.Disposed || !frustum.Intersects(obj.GetWorldBounds())) continue;
            pass.SetUniformBuffer(ObjectShaderDataBuffer, 0, (uint) (i * sizeof(ObjectShaderData)));
            obj.Draw(pass, passType);
        }
    }

    internal IEnumerable<(int Index, float Depth)> GetTransparentObjects()
    {
        BoundingFrustum3D frustum = CreateFrustum();
        for (int i = 0; i < Objects.Count; i++)
        {
            Object3D obj = Objects[i];
            if (!obj.Visible || obj.Disposed || !obj.HasTransparentContent || !frustum.Intersects(obj.GetWorldBounds())) continue;
            Vector3 worldPosition = Vector3.Transform(Vector3.Zero, obj.GetModelMatrix());
            float depth = Vector3.Dot(worldPosition - Camera.Position, Camera.Front);
            yield return (i, depth);
        }
    }

    internal unsafe void DrawObject(IRenderPass pass, int index, RenderPass passType)
    {
        if (!Visible || Disposed) return;
        BindSceneData(pass);
        pass.SetUniformBuffer(ObjectShaderDataBuffer, 0, (uint)(index * sizeof(ObjectShaderData)));
        Objects[index].Draw(pass, passType);
    }

    internal BoundingFrustum3D CreateFrustum() => new(Camera.GetViewMatrix() * Camera.GetProjectionMatrix());

    /// <summary>
    /// Gets the view matrix for the 3D scene, which is obtained from the camera.
    /// </summary>
    /// <returns>The view matrix for the 3D scene.</returns>
    protected override Matrix4x4 GetViewMatrix() => Camera.GetViewMatrix();

    /// <summary>
    /// Gets the projection matrix for the 3D scene, which is obtained from the camera.
    /// </summary>
    /// <returns>The projection matrix for the 3D scene.</returns>
    protected override Matrix4x4 GetProjectionMatrix() => Camera.GetProjectionMatrix();

    /// <summary>
    /// Releases the scene's objects, lights, and lighting buffers.
    /// </summary>
    public override void Dispose()
    {
        if (Disposed) return;
        foreach (Light light in _lights)
            light.DetachFrom(this);
        _lights.Clear();
        base.Dispose();
        _lighting.Dispose();
    }
}
