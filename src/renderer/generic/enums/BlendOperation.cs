namespace odl3d.Renderer;

/// <summary>
/// Represents the different blend operations that can be used in blending.
/// </summary>
public enum BlendOperation
{
    /// <summary>
    /// Represents an addition blend operation.
    /// </summary>
    Add,
    /// <summary>
    /// Represents a subtraction blend operation.
    /// </summary>
    Subtract,
    /// <summary>
    /// Represents a reverse subtraction blend operation.
    /// </summary>
    ReverseSubtract,
    /// <summary>
    /// Represents a minimum blend operation.
    /// </summary>
    Min,
    /// <summary>
    /// Represents a maximum blend operation.
    /// </summary>
    Max
}