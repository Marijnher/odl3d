using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// A light that radiates equally in all directions from a single position.
/// </summary>
public class PointLight : Light
{
    /// <summary>
    /// The position of the light in world space.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The distance at which the light has faded to zero. Zero or less means the light never fully fades.
    /// </summary>
    public float Range;

    /// <summary>
    /// Creates a white point light at the origin.
    /// </summary>
    public PointLight() { }

    /// <summary>
    /// Creates a point light.
    /// </summary>
    public PointLight(Vector3 position, Color? color = null, float intensity = 1f, float range = 0f)
    {
        Position = position;
        Color = color ?? Color.White;
        Intensity = intensity;
        Range = range;
    }

    internal override LightShaderData ToShaderData(Vector3 sceneOffset) => new()
    {
        PositionKind = new Vector4(Position + sceneOffset, (float) LightKind.Point),
        ColorRange = new Vector4(GetEmission(), MathF.Max(Range, 0f))
    };

    internal override float GetPriority(Vector3 worldPosition, Vector3 sceneOffset) =>
        Luminance(GetEmission()) * DistanceFalloff(Vector3.Distance(Position + sceneOffset, worldPosition));
}
