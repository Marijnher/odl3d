using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// A light that shines in a cone from a single position.
/// </summary>
public class SpotLight : Light
{
    private const float MinAngle = 0.01f;
    private const float MaxAngle = 89.99f;

    /// <summary>
    /// The position of the light in world space.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The direction the cone points in, in world space. It does not need to be normalized.
    /// </summary>
    public Vector3 Direction = new(0f, -1f, 0f);

    /// <summary>
    /// The distance at which the light has faded to zero. Zero or less means the light never fully fades.
    /// </summary>
    public float Range;

    /// <summary>
    /// The half-angle in degrees of the fully lit inner cone.
    /// </summary>
    public float InnerAngle = 20f;

    /// <summary>
    /// The half-angle in degrees of the outer cone, beyond which there is no light. The light fades smoothly between the inner and outer cone.
    /// </summary>
    public float OuterAngle = 30f;

    /// <summary>
    /// Creates a white spot light at the origin pointing straight down.
    /// </summary>
    public SpotLight() { }

    /// <summary>
    /// Creates a spot light.
    /// </summary>
    public SpotLight(Vector3 position, Vector3 direction, Color? color = null, float intensity = 1f, float range = 0f)
    {
        Position = position;
        Direction = direction;
        Color = color ?? Color.White;
        Intensity = intensity;
        Range = range;
    }

    internal override LightShaderData ToShaderData(Vector3 sceneOffset)
    {
        float outer = Math.Clamp(OuterAngle, MinAngle * 2f, MaxAngle);
        float inner = Math.Clamp(InnerAngle, 0f, outer - MinAngle);
        return new()
        {
            PositionKind = new Vector4(Position + sceneOffset, (float) LightKind.Spot),
            ColorRange = new Vector4(GetEmission(), MathF.Max(Range, 0f)),
            DirectionA = new Vector4(NormalizeOr(Direction, -Vector3.UnitY), MathF.Cos(outer * MathF.PI / 180f)),
            TangentB = new Vector4(Vector3.Zero, MathF.Cos(inner * MathF.PI / 180f))
        };
    }

    internal override float GetPriority(Vector3 worldPosition, Vector3 sceneOffset) =>
        Luminance(GetEmission()) * DistanceFalloff(Vector3.Distance(Position + sceneOffset, worldPosition));
}
