using System.Numerics;

namespace odl3d;

/// <summary>
/// A light infinitely far away, such as the sun. It lights everything from the same direction and does not fade with distance.
/// </summary>
public class DirectionalLight : Light
{
    /// <summary>
    /// The direction the light travels in, in world space. It does not need to be normalized.
    /// </summary>
    public Vector3 Direction = new(0f, -1f, 0f);

    /// <summary>
    /// Creates a white light pointing straight down.
    /// </summary>
    public DirectionalLight() { }

    /// <summary>
    /// Creates a directional light.
    /// </summary>
    public DirectionalLight(Vector3 direction, Color? color = null, float intensity = 1f)
    {
        Direction = direction;
        Color = color ?? Color.White;
        Intensity = intensity;
    }

    internal override LightShaderData ToShaderData(Vector3 sceneOffset) => new()
    {
        PositionKind = new Vector4(Vector3.Zero, (float) LightKind.Directional),
        ColorRange = new Vector4(GetEmission(), 0f),
        DirectionA = new Vector4(NormalizeOr(Direction, -Vector3.UnitY), 0f)
    };

    // Directional lights reach everything, so they are never dropped.
    internal override float GetPriority(Vector3 worldPosition, Vector3 sceneOffset) => float.MaxValue;
}
