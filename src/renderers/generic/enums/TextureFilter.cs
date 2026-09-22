namespace odl3d.Renderer;

/// <summary>
/// Defines the texture filtering modes used when sampling a texture at a size other than its native resolution.
/// </summary>
public enum TextureFilter
{
    /// <summary>
    /// Samples the single nearest texel; blocky but crisp, suited to pixel art.
    /// </summary>
    Nearest,

    /// <summary>
    /// Bilinearly interpolates between neighboring texels; smoother, suited to anti-aliased content like text.
    /// </summary>
    Linear
}
