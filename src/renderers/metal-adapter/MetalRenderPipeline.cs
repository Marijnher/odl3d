using System;
using System.Collections.Generic;
using System.IO;

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

        string vertexSource = description.VertexShader.Source;
        if (description.VertexShader.SourceAsFilename)
            vertexSource = File.ReadAllText(vertexSource);

        string fragmentSource = description.FragmentShader.Source;
        if (description.FragmentShader.SourceAsFilename)
            fragmentSource = File.ReadAllText(fragmentSource);

        Pipeline = Device.CreatePipeline(
            vertexDescriptor,
            vertexSource,
            fragmentSource,
            description.VertexShader.EntryPoint,
            description.FragmentShader.EntryPoint,
            description.Blend
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