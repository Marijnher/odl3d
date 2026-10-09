using System;
using System.Numerics;

namespace odl3d.Loaders;

/// <summary>
/// Surface lighting properties read from a model file (DAE effect or MTL material).
/// </summary>
/// <param name="Emissive">The color the surface glows with by itself.</param>
/// <param name="Specular">The color of the specular highlight; black disables it.</param>
/// <param name="Shininess">The specular exponent.</param>
/// <param name="Diffuse">The base color for surfaces without a texture; null when the file gives none or the surface is textured.</param>
public sealed record SourceMaterial(Color Emissive, Color Specular, float Shininess, Color? Diffuse = null)
{
    /// <summary>
    /// Converts a 0-1 float RGB triplet to a color, clamping out-of-range values.
    /// </summary>
    internal static Color ToColor(float r, float g, float b) => new(ToByte(r), ToByte(g), ToByte(b));

    private static byte ToByte(float value) => (byte) Math.Clamp(MathF.Round(value * 255f), 0f, 255f);
}

/// <summary>
/// The kinds of light a model file can define.
/// </summary>
public enum SourceLightKind
{
    Point,
    Directional,
    Spot
}

/// <summary>
/// A light defined in a model file. The transform is local to the model; light travels along the local -Z axis for directional and spot lights, as in Collada.
/// </summary>
public sealed record SourceLight(SourceLightKind Kind, Color Color, Matrix4x4 Transform, float OuterAngle = 45f)
{
    /// <summary>
    /// Creates the scene light for this definition, positioned for the given model matrix.
    /// </summary>
    /// <param name="model">The model's world matrix.</param>
    /// <param name="sceneOffset">The owning scene's position, which lights add on top of their own.</param>
    internal Light CreateLight(Matrix4x4 model, Vector3 sceneOffset)
    {
        Light light = Kind switch
        {
            SourceLightKind.Directional => new DirectionalLight { Color = Color },
            SourceLightKind.Spot => new SpotLight { Color = Color, OuterAngle = OuterAngle, InnerAngle = OuterAngle * 0.7f },
            _ => new PointLight { Color = Color }
        };
        Update(light, model, sceneOffset);
        return light;
    }

    /// <summary>
    /// Moves a light created by <see cref="CreateLight"/> to follow the model.
    /// </summary>
    internal void Update(Light light, Matrix4x4 model, Vector3 sceneOffset)
    {
        Matrix4x4 world = Transform * model;
        Vector3 position = world.Translation - sceneOffset;
        Vector3 direction = Light.NormalizeOr(Vector3.TransformNormal(new Vector3(0f, 0f, -1f), world), new Vector3(0f, -1f, 0f));
        switch (light)
        {
            case PointLight point: point.Position = position; break;
            case SpotLight spot: spot.Position = position; spot.Direction = direction; break;
            case DirectionalLight directional: directional.Direction = direction; break;
        }
    }
}
