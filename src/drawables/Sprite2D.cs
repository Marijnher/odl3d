using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using System;

namespace odl3d;

/// <summary>
/// A 2D sprite that is drawn in pixel coordinates relative to the top-left of the viewport. The sprite's quad is scaled to match the dimensions of its texture, and its position is specified in pixel coordinates.
/// The sprite is unaffected by any 3D camera and is drawn using an orthographic projection matrix based on the viewport rectangle of its Scene2D. A Window can display any number of Sprite2D instances at once, each in its own Scene2D with its own viewport.
/// </summary>
public class Sprite2D : Object3D
{
    /// <summary>
    /// Creates a new Sprite2D in the given Scene2D with an optional texture. The sprite's quad is scaled to match the dimensions of the texture, and its position is specified in pixel coordinates relative to the top-left of the viewport.
    /// </summary>
    /// <param name="scene">The Scene2D to which this sprite belongs.</param>
    /// <param name="texture">The optional texture to use for the sprite.</param>
    public Sprite2D(Scene<Object3D> scene, Texture? texture = null) : base(scene, MeshBuilder.CreateQuad(), texture)
    {
        if (scene is not Scene2D) Console.WriteLine("Warning: Sprite2D is being added to a Scene that is not a Scene2D. This may cause rendering issues.");
        AutoDisposeMesh = false;
    }

    /// <summary>
    /// Calculates and returns the model matrix for the sprite based on its position and texture size. The model matrix scales the quad to match the texture dimensions and translates it to the sprite's position in pixel coordinates relative to the top-left of the viewport.
    /// Position.Z is passed straight through as the world Z coordinate: it has no effect on X/Y screen placement, but a higher Z wins the GPU depth test (GL_LESS) against lower/overlapping sprites, so draw order falls out of the existing depth buffer instead of a per-frame sort.
    /// </summary>
    /// <returns>The model matrix for the sprite.</returns>
    public override Matrix4x4 GetModelMatrix() =>
        Matrix4x4.CreateScale(Texture!.Width * Scale.X, Texture!.Height * Scale.Y, 1f) *
        Matrix4x4.CreateTranslation(Position.X, Position.Y, Position.Z);

    /// <summary>
    /// Registers a callback to be invoked when the specified mouse button is pressed inside the sprite's bounds.
    /// </summary>
    /// <param name="button">The mouse button to listen for.</param>
    /// <param name="onPress">The callback to invoke when the mouse button is pressed inside the sprite's bounds. The callback receives the mouse position as a Vector2.</param>
    public void RegisterMousePressInside(Mouse button, Action<Vector2> onPress)
    {
        RegisterMousePress(button, (pos) =>
        {
            if (pos.X >= Position.X && pos.X <= Position.X + Texture!.Width * Scale.X &&
                pos.Y >= Position.Y && pos.Y <= Position.Y + Texture!.Height * Scale.Y)
            {
                onPress(pos);
            }
        });
    }
}

