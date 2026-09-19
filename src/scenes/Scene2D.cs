using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// A 2D scene containing a collection of Sprite2D instances and a viewport rectangle defining the area of the window in which the sprites will be rendered. The scene provides methods to add and remove sprites, and an implementation of the Draw method that renders all sprites in the scene using an orthographic projection matrix calculated from the viewport.
/// </summary>
public class Scene2D : Scene<Object3D>
{
    /// <summary>
    /// The viewport rectangle in pixel coordinates relative to the top-left of the window. Sprites are positioned within this rectangle, and the projection matrix is calculated based on this viewport.
    /// </summary>
    public Rect Viewport { get; set; }

    private readonly bool fitToWindow;

    /// <summary>
    /// Creates a new Scene2D with the given viewport rectangle. The viewport defines the area of the window in which the scene's sprites will be rendered, and the projection matrix is calculated based on this rectangle.
    /// </summary>
    /// <param name="window">The window in which the scene will be rendered.</param>
    /// <param name="viewport">The viewport rectangle in pixel coordinates relative to the top-left of the window.</param>
    public Scene2D(Window window, Rect viewport) : this(window, viewport, false) { }

    private Scene2D(Window window, Rect viewport, bool fitToWindow) : base(window)
    {
        this.Viewport = viewport;
        this.fitToWindow = fitToWindow;
        this.Window.AddScene(this);
    }

    /// <summary>
    /// Creates a new Scene2D with a viewport that matches the size of the given window. The viewport is set to (0, 0, window.Width, window.Height), and sprites will be positioned in pixel coordinates relative to the top-left of the window.
    /// </summary>
    /// <param name="window">The window whose size defines the viewport for the scene.</param>
    public Scene2D(Window window) : this(window, new Rect(0, 0, window.Width, window.Height), true) { }

    internal void UpdateWindowSize()
    {
        if (fitToWindow)
            Viewport = new Rect(0, 0, Window.Width, Window.Height);
    }

    /// <summary>
    /// Calculates and returns the orthographic projection matrix for the scene based on its viewport rectangle. The projection matrix maps pixel coordinates within the viewport to normalized device coordinates for rendering.
    /// Depth range is deliberately wide so sprites can use Position.Z purely as a draw-order index (see Sprite2D) without being clipped.
    /// </summary>
    /// <returns>The orthographic projection matrix for the scene.</returns>
    public Matrix4x4 GetProjectionMatrix() =>
        Matrix4x4.CreateOrthographicOffCenter(0, Viewport.Width, Viewport.Height, 0, -1000, 1000);

    /// <summary>
    /// Draws all sprites in the scene using the given shader. The projection matrix is calculated based on the scene's viewport, and each sprite is drawn in pixel coordinates relative to the top-left of the viewport.
    /// </summary>
    /// <param name="shader">The shader to use for drawing the sprites.</param>
    /// <param name="renderPass">The render pass to use for rendering the scene.</param>
    public override void Draw(IRenderPass pass, RenderPass passType = RenderPass.Opaque)
    {
        if (!Visible || Disposed) return;
        
        float scaleX = (float) Window.RenderSurface.Width / Window.Width;
        float scaleY = (float) Window.RenderSurface.Height / Window.Height;
        int vpX = (int) ((Viewport.X + Position.X) * scaleX);
        int vpY = (int) ((Viewport.Y + Position.Y) * scaleY);
        int vpWidth = (int) (Viewport.Width * scaleX);
        int vpHeight = (int) (Viewport.Height * scaleY);
        pass.SetViewport(new Rect(vpX, Window.RenderSurface.Height - vpY - vpHeight, vpWidth, vpHeight));
        // Draw in reverse order so the last-added sprite is drawn on top of other sprites with equal z values.
        for (int i = Objects.Count - 1; i >= 0; i--)
        {
            pass.SetVertexBuffer(ObjectShaderDataBuffer, 2, (uint) (i * ObjectShaderData.NumFloats));
            Objects[i].Draw(pass, passType);
        }
    }
}
