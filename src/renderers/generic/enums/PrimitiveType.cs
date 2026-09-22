namespace odl3d.Renderer;

/// <summary>
/// Represents the different types of primitives that can be used in rendering operations.
/// </summary>
public enum PrimitiveType
{
    /// <summary>
    /// Represents a list of points.
    /// </summary>
    PointList,
    
    /// <summary>
    /// Represents a list of lines, where each line is defined by two vertices.
    /// </summary>
    LineList,
    /// <summary>
    /// Represents a connected strip of lines, where each line shares a vertex with the previous line.
    /// </summary>
    LineStrip,

    /// <summary>
    /// Represents a list of triangles, where each triangle is defined by three vertices.
    /// </summary>
    TriangleList,
    /// <summary>
    /// Represents a connected strip of triangles, where each triangle shares vertices with the previous triangle.
    /// </summary>
    TriangleStrip
}