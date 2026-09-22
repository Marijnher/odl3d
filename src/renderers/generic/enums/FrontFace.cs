namespace odl3d.Renderer;

/// <summary>
/// Represents the winding order of front-facing polygons in rendering operations.
/// </summary>
public enum FrontFace
{
    /// <summary>
    /// Front-facing polygons are defined in a clockwise winding order.
    /// </summary>
    Clockwise,
    /// <summary>
    /// Front-facing polygons are defined in a counter-clockwise winding order.
    /// </summary>
    CounterClockwise
}