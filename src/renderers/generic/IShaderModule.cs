using System;

namespace odl3d.Renderer;

public interface IShaderModule : IGPUResource
{
    ShaderStage Stage { get; }
    string Source { get; }
    string EntryPoint { get; }
    ShaderLanguage ShaderLanguage { get; }
}