namespace odl3d;

/// <summary>
/// Represents the format of a vertex attribute, specifying the type and number of components.
/// </summary>
public enum VertexFormat
{
    /// <summary>
    /// A single-component floating-point vertex attribute.
    /// </summary>
    Float1,
    /// <summary>
    /// A two-component floating-point vertex attribute.
    /// </summary>
    Float2,
    /// <summary>
    /// A three-component floating-point vertex attribute.
    /// </summary>
    Float3,
    /// <summary>
    /// A four-component floating-point vertex attribute.
    /// </summary>
    Float4,

    /// <summary>
    /// A two-component half-precision floating-point vertex attribute.
    /// </summary>
    Half2,
    /// <summary>
    /// A four-component half-precision floating-point vertex attribute.
    /// </summary>
    Half4,

    /// <summary>
    /// A single-component integer vertex attribute.
    /// </summary>
    Int1,
    /// <summary>
    /// A two-component integer vertex attribute.
    /// </summary>
    Int2,
    /// <summary>
    /// A three-component integer vertex attribute.
    /// </summary>
    Int3,
    /// <summary>
    /// A four-component integer vertex attribute.
    /// </summary>
    Int4,

    /// <summary>
    /// A single-component unsigned integer vertex attribute.
    /// </summary>
    UInt1,
    /// <summary>
    /// A two-component unsigned integer vertex attribute.
    /// </summary>
    UInt2,
    /// <summary>
    /// A three-component unsigned integer vertex attribute.
    /// </summary>
    UInt3,
    /// <summary>
    /// A four-component unsigned integer vertex attribute.
    /// </summary>
    UInt4
}