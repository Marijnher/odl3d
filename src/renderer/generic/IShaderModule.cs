using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents a shader module that encapsulates the shader stage, source code, entry point, and shader language.
/// </summary>
public interface IShaderModule : IGPUResource
{
    /// <summary>
    /// Gets the shader stage of the shader module.
    /// </summary>
    ShaderStage Stage { get; }

    /// <summary>
    /// Gets the source code of the shader module.
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Gets a value indicating whether the source is provided as a filename.
    /// </summary>
    bool SourceAsFilename { get; }

    /// <summary>
    /// Gets the entry point of the shader module.
    /// </summary>
    string EntryPoint { get; }

    /// <summary>
    /// Gets the shader language of the shader module.
    /// </summary>
    ShaderLanguage ShaderLanguage { get; }
}