using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// True 3D text. Each glyph's vector outline is flattened into contours, the contours are classified into
/// filled shapes and holes, tessellated into front and back caps, and swept along Z into side walls, so the
/// text is real solid geometry: it can be lit, viewed from any angle, and intersected by other geometry,
/// rather than being a picture of text on a quad (use TextBillboard for that).
/// The geometry is built centered on the origin; Position.X identifies the left edge of the text block, while
/// Position.Y and Position.Z identify its center. Since there is no texture, Color tints the whole solid.
/// </summary>
public class Text3D : Text
{
    private float _depth;
    private float _pixelsPerWorldUnit;
    private float _smoothingAngle;

    /// <summary>
    /// Extrusion thickness along Z, in world units. Zero produces a single flat, infinitely thin face with no
    /// side walls or back cap.
    /// </summary>
    public float Depth
    {
        get => _depth;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if (_depth == value) return;
            _depth = value;
            Rebuild();
        }
    }

    /// <summary>
    /// How many font pixels make up one world unit. The font's pixel size controls how finely the outlines are
    /// flattened and hinted; this controls how large the resulting solid is in the scene.
    /// </summary>
    public float PixelsPerWorldUnit
    {
        get => _pixelsPerWorldUnit;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0f);
            if (_pixelsPerWorldUnit == value) return;
            _pixelsPerWorldUnit = value;
            Rebuild();
        }
    }

    /// <summary>
    /// Maximum angle, in degrees, between two adjacent side-wall faces that still gets a shared (smoothed)
    /// normal. Raise it to make the extruded edges rounder, lower it to make every segment read as a facet.
    /// </summary>
    public float SmoothingAngle
    {
        get => _smoothingAngle;
        set
        {
            if (_smoothingAngle == value) return;
            _smoothingAngle = value;
            Rebuild();
        }
    }

    /// <summary>The width of the text block in world units, before Scale.</summary>
    public float Width { get; private set; }

    /// <summary>The height of the text block in world units, before Scale.</summary>
    public float Height { get; private set; }

    /// <summary>The full bounding size of the text in world units, before Scale.</summary>
    public Vector3 Size => new Vector3(Width, Height, Depth);

    /// <summary>
    /// Creates extruded 3D text in the given scene.
    /// </summary>
    /// <param name="scene">The scene to which this text belongs.</param>
    /// <param name="font">The font whose outlines this text is built from.</param>
    /// <param name="content">The initial text to display. May contain line breaks.</param>
    /// <param name="style">The initial Bold/Italic style.</param>
    /// <param name="align">Horizontal alignment of each line within the text block.</param>
    /// <param name="depth">Extrusion thickness along Z, in world units.</param>
    /// <param name="pixelsPerWorldUnit">How many font pixels map to one world unit.</param>
    /// <param name="smoothingAngle">Angle threshold in degrees for smoothing the side-wall normals.</param>
    public Text3D(Scene<Object> scene, Font font, string content = "", FontStyle style = FontStyle.Regular, TextAlign align = TextAlign.Left, float depth = 0.05f, float pixelsPerWorldUnit = 256f, float smoothingAngle = 40f)
        : base(scene, font, content, style, align)
    {
        if (scene is not Scene3D) Console.WriteLine("Warning: Text3D is being added to a Scene that is not a Scene3D. This may cause rendering issues.");
        ArgumentOutOfRangeException.ThrowIfNegative(depth);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pixelsPerWorldUnit, 0f);
        _depth = depth;
        _pixelsPerWorldUnit = pixelsPerWorldUnit;
        _smoothingAngle = smoothingAngle;
        Color = Color.White;
        Rebuild();
    }

    /// <inheritdoc/>
    public override Matrix4x4 GetModelMatrix() =>
        Matrix4x4.CreateScale(Scale) *
        Matrix4x4.CreateRotationX(MathF.PI / 180 * Rotation.X) *
        Matrix4x4.CreateRotationY(MathF.PI / 180 * Rotation.Y) *
        Matrix4x4.CreateRotationZ(MathF.PI / 180 * Rotation.Z) *
        Matrix4x4.CreateTranslation(
            Position.X + Scene.Position.X + Width * Scale.X / 2f,
            Position.Y + Scene.Position.Y,
            Position.Z + Scene.Position.Z);

    /// <inheritdoc/>
    protected override void Rebuild()
    {
        TextGeometry.Result geometry = TextGeometry.Build(Font, Content, Style, Align, Underline, Strikethrough, _depth, _pixelsPerWorldUnit, _smoothingAngle);
        Width = geometry.Width;
        Height = geometry.Height;

        Mesh? old = Mesh;
        Mesh = geometry.Indices.Length > 0 ? new Mesh(geometry.Vertices, geometry.Indices, true) : null;
        old?.Dispose();
    }
}
