using System;

namespace odl3d.Renderer;

public interface IShaderModule : IGPUResource
{
    ShaderStage Stage { get; }
    string EntryPoint { get; }
}