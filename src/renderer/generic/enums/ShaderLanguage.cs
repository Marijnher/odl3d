namespace odl3d.Renderer;

/// <summary>
/// Represents the different shader languages that can be used in rendering operations.
/// </summary>
public enum ShaderLanguage
{
    /// <summary>
    /// The default shader language.
    /// </summary>
    Default,
    /// <summary>
    /// The OpenGL Shading Language (GLSL).
    /// </summary>
    GLSL,
    /// <summary>
    /// The Metal Shading Language (MSL).
    /// </summary>
    MSL,
    /// <summary>
    /// The High-Level Shading Language (HLSL).
    /// </summary>
    HLSL,
    /// <summary>
    /// The SPIR-V intermediate language.
    /// </summary>
    SPIRV
}