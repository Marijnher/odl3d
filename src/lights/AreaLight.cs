using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// The outline of an <see cref="AreaLight"/>.
/// </summary>
public enum AreaLightShape
{
    /// <summary>
    /// A rectangle of <see cref="AreaLight.Width"/> by <see cref="AreaLight.Height"/>.
    /// </summary>
    Rectangle,

    /// <summary>
    /// An ellipse that fits inside the same width and height; a circle when they are equal.
    /// </summary>
    Disc
}

/// <summary>
/// A flat surface that emits light, such as a ceiling panel or a window. Unlike a point light its highlight and falloff follow the shape of the surface.
/// </summary>
/// <remarks>
/// The light is approximated: each shaded point is lit as if by a point light placed at the closest point on the surface. This is cheap and looks right for soft area lighting, but it does not cast shadows and its specular highlight is only an approximation of a true area reflection.
/// The light is not visible by itself; pair it with a bright, emissive object (<see cref="Object3D.Emissive"/>) of the same size and placement to show the emitting surface.
/// </remarks>
public class AreaLight : Light
{
    /// <summary>
    /// The center of the surface in world space.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The orientation as Euler angles in degrees, applied like <see cref="Drawable.Rotation"/>. Unrotated, the surface spans the X and Y axes and emits towards +Z.
    /// </summary>
    public Vector3 Rotation;

    /// <summary>
    /// The size of the surface along its local X axis.
    /// </summary>
    public float Width = 1f;

    /// <summary>
    /// The size of the surface along its local Y axis.
    /// </summary>
    public float Height = 1f;

    /// <summary>
    /// The outline of the surface.
    /// </summary>
    public AreaLightShape Shape = AreaLightShape.Rectangle;

    /// <summary>
    /// When true the surface emits from both faces instead of only the +Z side.
    /// </summary>
    public bool TwoSided;

    /// <summary>
    /// The distance at which the light has faded to zero. Zero or less means the light never fully fades.
    /// </summary>
    public float Range;

    /// <summary>
    /// Gets the direction the surface emits towards, in world space.
    /// </summary>
    public Vector3 Normal => Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, GetRotationMatrix()));

    /// <summary>
    /// Creates a white rectangular area light at the origin.
    /// </summary>
    public AreaLight() { }

    /// <summary>
    /// Creates an area light.
    /// </summary>
    public AreaLight(Vector3 position, float width, float height, Color? color = null, float intensity = 1f, AreaLightShape shape = AreaLightShape.Rectangle)
    {
        Position = position;
        Width = width;
        Height = height;
        Color = color ?? Color.White;
        Intensity = intensity;
        Shape = shape;
    }

    private Matrix4x4 GetRotationMatrix() =>
        Matrix4x4.CreateRotationX(MathF.PI / 180f * Rotation.X) *
        Matrix4x4.CreateRotationY(MathF.PI / 180f * Rotation.Y) *
        Matrix4x4.CreateRotationZ(MathF.PI / 180f * Rotation.Z);

    internal override LightShaderData ToShaderData(Vector3 sceneOffset)
    {
        Matrix4x4 rotation = GetRotationMatrix();
        Vector3 normal = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, rotation));
        Vector3 right = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitX, rotation));
        float kind = (float) (Shape == AreaLightShape.Disc ? LightKind.Disc : LightKind.Rectangle) + (TwoSided ? LightShaderData.TwoSidedFlag : 0);
        return new()
        {
            PositionKind = new Vector4(Position + sceneOffset, kind),
            ColorRange = new Vector4(GetEmission(), MathF.Max(Range, 0f)),
            DirectionA = new Vector4(normal, MathF.Max(Width, 0f) * 0.5f),
            TangentB = new Vector4(right, MathF.Max(Height, 0f) * 0.5f)
        };
    }

    internal override float GetPriority(Vector3 worldPosition, Vector3 sceneOffset) =>
        Luminance(GetEmission()) * DistanceFalloff(Vector3.Distance(Position + sceneOffset, worldPosition));
}
