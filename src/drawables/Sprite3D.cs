using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// A 3D sprite is a simple quad mesh with a texture applied, used for billboarding or other 3D effects. Its
/// Position.X identifies the left edge of the quad; Position.Y and Position.Z identify its center.
/// </summary>
public class Sprite3D : Object3D
{
    /// <summary>
    /// Creates a new Sprite3D with the given texture. The sprite uses a built-in quad mesh and can be positioned,
    /// scaled, and rotated in 3D space like any other Object.
    /// </summary>
    /// <param name="scene">The scene to which this sprite belongs.</param>
    /// <param name="texture">The texture to use for the sprite.</param>
    public Sprite3D(Scene<Object3D> scene, Texture? texture = null) : base(scene, MeshBuilder.CreateQuad(), texture) { }

    /// <inheritdoc/>
    public override Matrix4x4 GetModelMatrix() =>
        Matrix4x4.CreateScale(Scale) *
        Matrix4x4.CreateRotationX(MathF.PI / 180 * Rotation.X) *
        Matrix4x4.CreateRotationY(MathF.PI / 180 * Rotation.Y) *
        Matrix4x4.CreateRotationZ(MathF.PI / 180 * Rotation.Z) *
        Matrix4x4.CreateTranslation(Position.X + Scene.Position.X + Scale.X / 2f, Position.Y + Scene.Position.Y, Position.Z + Scene.Position.Z);
}
