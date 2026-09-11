/// <summary>
/// Represents the levels of anisotropic filtering available for textures.
/// </summary>
public enum AnisotropicFilter
{
    /// <summary>
    /// No anisotropic filtering.
    /// </summary>
    None    = 1,

    /// <summary>
    /// Anisotropic filtering with a 2x sample rate.
    /// </summary>
    X2      = 2,

    /// <summary>
    /// Anisotropic filtering with a 4x sample rate.
    /// </summary>
    X4      = 4,

    /// <summary>
    /// Anisotropic filtering with an 8x sample rate.
    /// </summary>
    X8      = 8,

    /// <summary>
    /// Anisotropic filtering with a 16x sample rate.
    /// </summary>
    X16     = 16
}