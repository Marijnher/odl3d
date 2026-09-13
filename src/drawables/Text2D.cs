using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// Scene2D text sized in pixels. Position.X identifies the left edge of the text quad, while Position.Y and
/// Position.Z identify its center.
/// </summary>
public class Text2D : RasterizedText
{
    /// <summary>
    /// Creates screen-space text in the given 2D scene.
    /// </summary>
    /// <param name="scene">The scene to which this text belongs.</param>
    /// <param name="font">The font to measure and rasterize this text's glyphs with.</param>
    /// <param name="content">The initial text to display. May contain line breaks.</param>
    /// <param name="style">The initial Bold/Italic style.</param>
    /// <param name="align">Horizontal alignment of each line within the text block.</param>
    /// <param name="atlas">An optional custom GlyphAtlas; defaults to the shared process-wide atlas.</param>
    public Text2D(Scene<Object3D> scene, Font font, string content = "", FontStyle style = FontStyle.Regular, TextAlign align = TextAlign.Left, GlyphAtlas? atlas = null)
        : base(scene, font, content, style, align, atlas)
    {
        if (scene is not Scene2D) Console.WriteLine("Warning: Text2D is being added to a Scene that is not a Scene2D. This may cause rendering issues.");
        Mesh = MeshBuilder.CreateQuad();
        AutoDisposeMesh = false;
        Rebuild();
    }

    /// <inheritdoc/>
    public override Matrix4x4 GetModelMatrix()
    {
        float width = Texture?.Width ?? 0f;
        float height = Texture?.Height ?? 0f;

        return Matrix4x4.CreateScale(width * Scale.X, height * Scale.Y, 1f) *
            Matrix4x4.CreateRotationX(MathF.PI / 180 * Rotation.X) *
            Matrix4x4.CreateRotationY(MathF.PI / 180 * Rotation.Y) *
            Matrix4x4.CreateRotationZ(MathF.PI / 180 * Rotation.Z) *
            Matrix4x4.CreateTranslation(
                Position.X + Scene.Position.X,
                Position.Y + Scene.Position.Y,
                Position.Z + Scene.Position.Z);
    }
}