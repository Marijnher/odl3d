using System;

namespace odl3d.Renderer.MetalAdapter;

public class MetalShaderModule : IShaderModule
{
    public ShaderStage Stage { get; }
    public string Source { get; }
    public string EntryPoint { get; }
    public ShaderLanguage ShaderLanguage { get; }
    public bool Disposed { get; private set; }

    public MetalShaderModule(ShaderModuleDescription description)
    {
        Stage = description.Stage;
        EntryPoint = description.EntryPoint;
        Source = description.Source;
        ShaderLanguage = description.ShaderLanguage;
    }

    public void Dispose() => Disposed = true;
}