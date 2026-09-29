using System;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a Metal shader module and provides access to its properties.
/// </summary>
internal class MetalShaderModule : IShaderModule
{
    /// <summary>
    /// Gets the stage of the shader module.
    /// </summary>
    public ShaderStage Stage { get; }

    /// <summary>
    /// Gets the source code of the shader module.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets a value indicating whether the source is a filename.
    /// </summary>
    public bool SourceAsFilename { get; }

    /// <summary>
    /// Gets the entry point of the shader module.
    /// </summary>
    public string EntryPoint { get; }

    /// <summary>
    /// Gets the shading language of the shader module.
    /// </summary>
    public ShaderLanguage ShaderLanguage { get; }

    /// <summary>
    /// Gets a value indicating whether the shader module has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalShaderModule"/> class.
    /// </summary>
    /// <param name="description">The description of the shader module.</param>
    public MetalShaderModule(ShaderModuleDescription description)
    {
        Stage = description.Stage;
        Source = description.Source;
        SourceAsFilename = description.SourceAsFilename;
        EntryPoint = description.EntryPoint;
        ShaderLanguage = description.ShaderLanguage;
        if (SourceAsFilename) Source = System.IO.File.ReadAllText(Source);
    }

    /// <summary>
    /// Disposes the shader module and releases its resources.
    /// </summary>
    public void Dispose() => Disposed = true;
}