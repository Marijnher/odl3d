namespace odl3d;

/// <summary>
/// Defines the texture wrapping modes used when sampling textures outside the [0, 1] UV coordinate range.
/// </summary>
public enum TextureWrap
{
    /// <summary>
    /// Repeats the texture when sampling outside the [0, 1] UV coordinate range.
    /// </summary>
    Repeat,

    /// <summary>
    /// Clamps the texture coordinates to the edge of the texture, effectively stretching the edge pixels when sampling outside the [0, 1] UV coordinate range.
    /// </summary>
    Clamp,

    /// <summary>
    /// Mirrors the texture when sampling outside the [0, 1] UV coordinate range.
    /// </summary>
    Mirror
}