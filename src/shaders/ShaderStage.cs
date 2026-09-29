namespace odl3d;

/// <summary>
/// Represents the different stages of a shader in the rendering pipeline.
/// </summary>
public enum ShaderStage
{
    /// <summary>
    /// The vertex shader stage.
    /// </summary>
    Vertex,

    /// <summary>
    /// The fragment shader stage.
    /// </summary>
    Fragment,

    /// <summary>
    /// The compute shader stage.
    /// </summary>
    Compute,

    /// <summary>
    /// The geometry shader stage.
    /// </summary>
    Geometry,

    /// <summary>
    /// The tessellation control shader stage.
    /// </summary>
    TesselationControl,
    
    /// <summary>
    /// The tessellation evaluation shader stage.
    /// </summary>
    TesselationEvaluation
}