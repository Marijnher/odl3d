using System;

namespace odl3d.Renderer;

public sealed record ShaderModuleDescription
{
    public required ShaderStage Stage { get; init; }
    public required string Source { get; init; }
    public string? EntryPoint { get; init; }
    public ShaderLanguage ShaderLanguage { get; init; } = ShaderLanguage.Default;
}