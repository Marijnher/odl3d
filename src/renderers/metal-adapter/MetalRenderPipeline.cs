using System;

namespace odl3d.Renderer.MetalAdapter;

public class MetalRenderPipeline : IRenderPipeline
{
    private Metal.Device Device;

    public PrimitiveType PrimitiveType { get; }
    public Metal.RenderPipelineState Pipeline { get; }

    public bool Disposed { get; private set; }

    public MetalRenderPipeline(Metal.Device device, RenderPipelineDescription description)
    {
        Device = device;
        Pipeline = Device.CreatePipeline(
            description.VertexShader.Source,
            description.FragmentShader.Source,
            description.VertexShader.EntryPoint,
            description.FragmentShader.EntryPoint
        );
        PrimitiveType = description.PrimitiveType;
    }

    public void Dispose()
    {
        if (Disposed) return;
        Pipeline.Dispose();
        Disposed = true;
    }
}