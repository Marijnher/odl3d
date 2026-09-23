namespace odl3d.Renderer;

/// <summary>
/// Represents the different culling modes that can be used in rendering operations.
/// </summary>
public enum CullMode
{
    /// <summary>
    /// No culling is performed.
    /// </summary>
    None,
    /// <summary>
    /// Front-face polygons are culled.
    /// </summary>
    Front,
    /// <summary>
    /// Back-face polygons are culled.
    /// </summary>
    Back
}