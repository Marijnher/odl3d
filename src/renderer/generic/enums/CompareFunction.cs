namespace odl3d.Renderer;

/// <summary>
/// Represents the different comparison functions that can be used in rendering operations.
/// </summary>
public enum CompareFunction
{
    /// <summary>
    /// Represents a comparison function that never passes.
    /// </summary>
    Never,
    /// <summary>
    /// Represents a comparison function that passes if the first value is less than the second value.
    /// </summary>
    Less,
    /// <summary>
    /// Represents a comparison function that passes if the first value is equal to the second value.
    /// </summary>
    Equal,
    /// <summary>
    /// Represents a comparison function that passes if the first value is less than or equal to the second value.
    /// </summary>
    LessEqual,
    /// <summary>
    /// Represents a comparison function that passes if the first value is greater than the second value.
    /// </summary>
    Greater,
    /// <summary>
    /// Represents a comparison function that passes if the first value is not equal to the second value.
    /// </summary>
    NotEqual,
    /// <summary>
    /// Represents a comparison function that passes if the first value is greater than or equal to the second value.
    /// </summary>
    GreaterEqual,
    /// <summary>
    /// Represents a comparison function that always passes.
    /// </summary>
    Always
}