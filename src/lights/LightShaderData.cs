using System.Numerics;
using System.Runtime.InteropServices;

namespace odl3d;

/// <summary>
/// The kind of light a <see cref="LightShaderData"/> entry describes. The values are shared with the shaders.
/// </summary>
internal enum LightKind
{
    Point = 0,
    Directional = 1,
    Spot = 2,
    Rectangle = 3,
    Disc = 4
}

/// <summary>
/// GPU representation of one light. Every field is a Vector4 so the layout is identical under std140 and MSL.
/// </summary>
/// <remarks>
/// <see cref="PositionKind"/>: xyz = world position, w = <see cref="LightKind"/> plus <see cref="TwoSidedFlag"/>.
/// <see cref="ColorRange"/>: rgb = color multiplied by intensity, w = range (0 for unlimited).
/// <see cref="DirectionA"/>: xyz = direction (the way a directional or spot light points, the emitting normal of an area light),
/// w = cosine of the spot's outer angle or the area's half width.
/// <see cref="TangentB"/>: xyz = the area's right axis, w = cosine of the spot's inner angle or the area's half height.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Size = 64)]
internal struct LightShaderData
{
    /// <summary>
    /// Added to the kind when an area light emits from both faces.
    /// </summary>
    public const int TwoSidedFlag = 8;

    public Vector4 PositionKind;
    public Vector4 ColorRange;
    public Vector4 DirectionA;
    public Vector4 TangentB;
}

/// <summary>
/// GPU representation of the per-scene lighting state that accompanies the light list.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 48)]
internal struct LightingShaderData
{
    /// <summary>
    /// World-space camera position (xyz), used for specular highlights.
    /// </summary>
    public Vector4 CameraPosition;

    /// <summary>
    /// Ambient light color (rgb) added to every lit surface.
    /// </summary>
    public Vector4 Ambient;

    /// <summary>
    /// The number of valid entries in the light list.
    /// </summary>
    public uint LightCount;
}
