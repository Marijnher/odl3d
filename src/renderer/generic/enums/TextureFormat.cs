namespace odl3d.Renderer;

public enum TextureFormat
{
    /// <summary>
    /// Represents an 8-bit per channel RGBA texture with normalized unsigned integer values.
    /// </summary>
    RGBA8Unorm,
    /// <summary>
    /// Represents an 8-bit per channel BGRA texture with normalized unsigned integer values.
    /// </summary>
    BGRA8Unorm,
    /// <summary>
    /// Represents a 16-bit per channel RGBA texture with floating-point values.
    /// </summary>
    RGBA16Float,
    /// <summary>
    /// Represents a 32-bit per channel RGBA texture with floating-point values.
    /// </summary>
    RGBA32Float,

    /// <summary>
    /// Represents a 16-bit depth texture with normalized unsigned integer values.
    /// </summary>
    Depth16Unorm,
    /// <summary>
    /// Represents a 24-bit depth texture with an 8-bit stencil component, using normalized unsigned integer values.
    /// </summary>
    Depth24UnormStencil8,
    /// <summary>
    /// Represents a 32-bit depth texture with floating-point values.
    /// </summary>
    Depth32Float,
    /// <summary>
    /// Represents a 32-bit depth texture with floating-point values and an 8-bit stencil component.
    /// </summary>
    Depth32FloatStencil8
}