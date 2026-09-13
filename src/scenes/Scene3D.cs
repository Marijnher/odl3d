using System.Collections.Generic;
using System.Numerics;

namespace odl3d;

/// <summary>
/// A 3D scene containing a collection of Object instances and a Camera. The scene provides methods to add and remove objects, and an implementation of the Draw method that renders all objects in the scene using the camera's view and projection matrices.
/// </summary>
public class Scene3D : Scene<Object>
{
    /// <summary>
    /// The camera used to render the scene. The camera's view and projection matrices are combined to create the view-projection matrix used for rendering the objects in the scene. The camera can be configured with position, orientation, field of view, aspect ratio, and other properties to control how the scene is viewed.
    /// </summary>
    /// <param name="window">The window associated with the scene, used to determine the rendering context and other properties.</param>
    public Scene3D(Window window) : base(window)
    {
        this.Window.AddScene(this);
    }

    /// <summary>
    /// Draws all objects in the scene using the given shader. Runs both the opaque and transparent pass back to back for this scene alone; prefer DrawPass via Window.Render when multiple Scene3D instances share a window, so transparency blends correctly against every scene's opaque geometry rather than just this scene's.
    /// </summary>
    /// <param name="shader">The shader program to use for rendering the scene.</param>
    /// <param name="renderPass">The render pass to use for rendering the scene.</param>
    public override void Draw(Shader shader, RenderPass renderPass = RenderPass.Opaque)
    {
        if (!Visible || Disposed) return;
        Matrix4x4 viewProjection = Camera.GetViewMatrix() * Camera.GetProjectionMatrix();
        foreach (Object sceneObject in Objects)
            sceneObject.Draw(shader, viewProjection, renderPass);
    }
}

