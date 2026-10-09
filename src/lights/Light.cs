using System;
using System.Collections.Generic;
using System.Numerics;

namespace odl3d;

/// <summary>
/// Base class for lights that illuminate the objects of a <see cref="Scene3D"/>. Lights are plain data: they own no GPU resources and need no disposal.
/// </summary>
public abstract class Light
{
    private readonly List<Scene3D> _scenes = new();

    /// <summary>
    /// The scenes this light has been added to. A light can be shared by any number of scenes; positional lights are offset by each scene's own position.
    /// </summary>
    public IReadOnlyList<Scene3D> Scenes => _scenes;

    internal void AttachTo(Scene3D scene) => _scenes.Add(scene);

    internal void DetachFrom(Scene3D scene) => _scenes.Remove(scene);

    /// <summary>
    /// The color of the emitted light.
    /// </summary>
    public Color Color = Color.White;

    /// <summary>
    /// A multiplier applied to <see cref="Color"/>. Values above 1 are allowed.
    /// </summary>
    public float Intensity = 1f;

    /// <summary>
    /// Disabled lights are ignored when rendering.
    /// </summary>
    public bool Enabled = true;

    /// <summary>
    /// The emitted color with intensity applied, as linear RGB.
    /// </summary>
    internal Vector3 GetEmission()
    {
        Vector4 color = Color.ToVector4();
        return new Vector3(color.X, color.Y, color.Z) * MathF.Max(Intensity, 0f);
    }

    /// <summary>
    /// Packs this light for the GPU.
    /// </summary>
    /// <param name="sceneOffset">The position of the owning scene, which offsets the world position of positional lights just like it does for objects.</param>
    internal abstract LightShaderData ToShaderData(Vector3 sceneOffset);

    /// <summary>
    /// Estimates how much this light contributes at a world position. It is only used to choose which lights to keep when a scene has more lights than a single draw can hold.
    /// </summary>
    internal abstract float GetPriority(Vector3 worldPosition, Vector3 sceneOffset);

    /// <summary>
    /// Gets the squared distance falloff used by positional lights, matching the shader's `1 / (1 + d^2)` term.
    /// </summary>
    internal static float DistanceFalloff(float distance) => 1f / (1f + distance * distance);

    internal static Vector3 NormalizeOr(Vector3 direction, Vector3 fallback) =>
        direction.LengthSquared() > 1e-12f ? Vector3.Normalize(direction) : fallback;

    internal static float Luminance(Vector3 color) => 0.2126f * color.X + 0.7152f * color.Y + 0.0722f * color.Z;
}
