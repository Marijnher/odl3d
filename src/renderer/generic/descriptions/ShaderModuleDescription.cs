using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the description of a shader module, including its stage, source code, entry point, and shader language.
/// </summary>
public sealed record ShaderModuleDescription
{
    /// <summary>
    /// The shader stage of the module (e.g., vertex, fragment, compute).
    /// </summary>
    public required ShaderStage Stage { get; init; }

    /// <summary>
    /// The source code of the shader module, or the filename if SourceAsFilename is true.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Indicates whether the Source property should be interpreted as a filename rather than raw shader code.
    /// </summary>
    public bool SourceAsFilename { get; init; } = false;
    
    /// <summary>
    /// The entry point function of the shader module.
    /// </summary>
    public required string EntryPoint { get; init; }

    /// <summary>
    /// The shading language used by the shader module.
    /// </summary>
    public ShaderLanguage ShaderLanguage { get; init; } = ShaderLanguage.Default;
}