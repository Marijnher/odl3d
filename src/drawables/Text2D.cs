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
    public Text2D(Font font, string content = "", FontStyle style = FontStyle.Regular, TextAlign align = TextAlign.Left, GlyphAtlas? atlas = null)
        : base(font, content, style, align, atlas)
    {
        Mesh = MeshBuilder.CreateQuad();
        AutoDisposeMesh = false;
        Rebuild();
    }

    /// <summary>
    /// Gets the model matrix for the 2D text, which defines its position, rotation, and scale in screen space.
    /// </summary>
    /// <returns>The model matrix representing the text's transformation in screen space.</returns>
    public override Matrix4x4 GetModelMatrix()
    {
        float width = Texture?.Width ?? 0f;
        float height = Texture?.Height ?? 0f;

        return Matrix4x4.CreateScale(width * Scale.X, height * Scale.Y, 1f) *
            Matrix4x4.CreateRotationX(MathF.PI / 180 * Rotation.X) *
            Matrix4x4.CreateRotationY(MathF.PI / 180 * Rotation.Y) *
            Matrix4x4.CreateRotationZ(MathF.PI / 180 * Rotation.Z) *
            Matrix4x4.CreateTranslation(
                Position.X + (Scene?.Position.X ?? 0f),
                Position.Y + (Scene?.Position.Y ?? 0f),
                Position.Z + (Scene?.Position.Z ?? 0f));
    }
}