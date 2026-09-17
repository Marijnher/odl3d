using System;
using System.Collections.Generic;

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
        var vertexDescriptor = Metal.VertexDescriptor.Create();
        var attribs = vertexDescriptor.Attributes;
        for (int i = 0; i < description.VertexLayout.Attributes.Length; i++)
        {
            var attrDescr = description.VertexLayout.Attributes[i];
            var attrib = attribs[(int) attrDescr.AttributeIndex];
            attrib.Format = attrDescr.Format;
            attrib.BufferIndex = attrDescr.BufferSlot;
            attrib.Offset = attrDescr.Offset;
        }

        var layouts = vertexDescriptor.Layouts;
        for (int i = 0; i < description.VertexLayout.Buffers.Length; i++)
        {
            var bufDescr = description.VertexLayout.Buffers[i];
            var layout = layouts[(int) bufDescr.BufferIndex];
            layout.Stride = bufDescr.Stride;
            layout.StepFunction = bufDescr.StepFunction;
        }

        Pipeline = Device.CreatePipeline(
            vertexDescriptor,
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