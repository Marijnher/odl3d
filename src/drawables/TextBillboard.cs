using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// World-space text drawn as a single textured quad, i.e. the 3D counterpart of Text2D. By default the quad
/// turns to face the camera every frame (a billboard); set FaceCamera to false to leave it oriented by
/// Rotation like any other object. Use Text3D instead when the text should be solid, extruded geometry.
/// Position.X identifies the left edge of the quad, while Position.Y and Position.Z identify its center.
/// </summary>
public class TextBillboard : RasterizedText
{
    private float _pixelsPerWorldUnit;

    /// <summary>
    /// How many rasterized font pixels make up one world unit. The font's pixel size controls how crisp the
    /// text is; this controls how large it is in the scene.
    /// </summary>
    public float PixelsPerWorldUnit
    {
        get => _pixelsPerWorldUnit;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0f);
            _pixelsPerWorldUnit = value;
        }
    }

    /// <summary>
    /// If true, the quad is rotated to face the camera each time it is drawn, ignoring Rotation. If false, it
    /// stays fixed in the scene and is oriented by Rotation like any other object.
    /// </summary>
    public bool FaceCamera = true;

    /// <summary>The width of this text in world units.</summary>
    public float Width => PixelWidth / PixelsPerWorldUnit;

    /// <summary>The height of this text in world units.</summary>
    public float Height => PixelHeight / PixelsPerWorldUnit;

    /// <summary>
    /// Creates world-space quad text in the given scene.
    /// </summary>
    /// <param name="scene">The scene to which this text belongs.</param>
    /// <param name="font">The font to measure and rasterize this text's glyphs with.</param>
    /// <param name="content">The initial text to display. May contain line breaks.</param>
    /// <param name="style">The initial Bold/Italic style.</param>
    /// <param name="align">Horizontal alignment of each line within the text block.</param>
    /// <param name="pixelsPerWorldUnit">How many rasterized font pixels map to one world unit.</param>
    /// <param name="atlas">An optional custom GlyphAtlas; defaults to the shared process-wide atlas.</param>
    public TextBillboard(Scene<Object> scene, Font font, string content = "", FontStyle style = FontStyle.Regular, TextAlign align = TextAlign.Left, float pixelsPerWorldUnit = 256f, GlyphAtlas? atlas = null)
        : base(scene, font, content, style, align, atlas)
    {
        if (scene is not Scene3D) Console.WriteLine("Warning: TextBillboard is being added to a Scene that is not a Scene3D. This may cause rendering issues.");
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pixelsPerWorldUnit, 0f);
        _pixelsPerWorldUnit = pixelsPerWorldUnit;
        Mesh = Mesh.Quad;
        AutoDisposeMesh = false;
        Rebuild();
    }

    /// <summary>
    /// Gets the model matrix for this text, which transforms its local coordinates to world coordinates. The model matrix is computed based on the text's position, rotation, scale, and the scene's position, and is used for rendering the text in the correct location and orientation within the scene.
    /// </summary>
    /// <returns>The model matrix for this text.</returns>
    public override Matrix4x4 GetModelMatrix()
    {
        float width = Width;
        float height = Height;

        return Matrix4x4.CreateScale(width * Scale.X, height * Scale.Y, 1f) *
            GetOrientation() *
            Matrix4x4.CreateTranslation(
                Position.X + Scene.Position.X + width * Scale.X / 2f,
                Position.Y + Scene.Position.Y,
                Position.Z + Scene.Position.Z);
    }

    /// <summary>
    /// Gets the orientation matrix for this text, which defines its rotation in world space. If FaceCamera is true, the orientation is aligned with the camera's axes to create a billboard effect; otherwise, it is based on the text's Rotation property. This matrix is used in conjunction with the model matrix to transform the text's local coordinates to world coordinates for rendering.
    /// </summary>
    /// <returns>The orientation matrix for this text.</returns>
    private Matrix4x4 GetOrientation()
    {
        if (!FaceCamera)
            return Matrix4x4.CreateRotationX(MathF.PI / 180 * Rotation.X) *
                Matrix4x4.CreateRotationY(MathF.PI / 180 * Rotation.Y) *
                Matrix4x4.CreateRotationZ(MathF.PI / 180 * Rotation.Z);

        // View-aligned billboard: map the quad's local axes onto the camera's, so it stays parallel to the
        // screen (no perspective distortion) rather than swivelling toward the camera's position.
        Camera camera = Scene.Window.Camera;
        Vector3 right = camera.Right;
        Vector3 up = camera.Up;
        Vector3 backward = -camera.Front;
        return new Matrix4x4(
            right.X, right.Y, right.Z, 0f,
            up.X, up.Y, up.Z, 0f,
            backward.X, backward.Y, backward.Z, 0f,
            0f, 0f, 0f, 1f);
    }

    /// <summary>
    /// Called when the texture for this text is rebuilt. This method is used to configure the texture's wrapping behavior.
    /// </summary>
    protected override void OnTextureRebuilt()
    {
        if (Texture == null) return;
        Texture.WrapModeH = TextureWrap.Clamp;
        Texture.WrapModeV = TextureWrap.Clamp;
    }
}
