namespace odl3d;

/// <summary>
/// Specifies the filter mode used for mipmaps when sampling textures.
/// </summary>
public enum MipmapFilter
{
    /// <summary>
    /// No mipmapping will be used.
    /// </summary>
    None = 0,

    /// <summary>
    /// Use the nearest mipmap that matches the pixel size.
    /// </summary>
    Nearest = 1,

    /// <summary>
    /// Interpolate between the two closest mipmaps.
    /// </summary>
    Linear = 2
}