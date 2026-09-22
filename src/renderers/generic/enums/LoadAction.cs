namespace odl3d.Renderer;

/// <summary>
/// Represents the different load actions that can be used in rendering operations.
/// </summary>
public enum LoadAction
{
    /// <summary>
    /// The load action is unspecified and the contents of the render target are undefined.
    /// </summary>
    DontCare,
    /// <summary>
    /// The load action loads the existing contents of the render target.
    /// </summary>
    Load,
    /// <summary>
    /// The load action clears the render target to a specified clear value.
    /// </summary>
    Clear
}