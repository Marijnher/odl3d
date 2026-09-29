using System;
using System.Collections.Generic;
using System.IO;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a Metal render pipeline, encapsulating the pipeline state and associated configurations.
/// </summary>
internal class MetalRenderPipeline : IRenderPipeline
{
    private Metal.Device Device;

    /// <summary>
    /// Gets the primitive type used by the render pipeline.
    /// </summary>
    public PrimitiveType PrimitiveType { get; }

    /// <summary>
    /// Gets the vertex layout description used by the render pipeline.
    /// </summary>
    public VertexLayoutDescription VertexLayout { get; }

    /// <summary>
    /// Gets the Metal render pipeline state object associated with this render pipeline.
    /// </summary>
    public Metal.RenderPipelineState Pipeline { get; }

    /// <summary>
    /// Gets a value indicating whether the render pipeline is configured for wireframe rendering.
    /// </summary>
    public bool Wireframe { get; set; }

    /// <summary>
    /// Gets a value indicating whether the render pipeline has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalRenderPipeline"/> class with the specified device and pipeline description.
    /// </summary>
    /// <param name="device">The Metal device used to create the render pipeline.</param>
    /// <param name="description">The description of the render pipeline, including shaders, vertex layout, and other configurations.</param>
    public MetalRenderPipeline(Metal.Device device, RenderPipelineDescription description)
    {
        Device = device;
        Wireframe = description.Wireframe;
        var vertexDescriptor = Metal.VertexDescriptor.Create();
        var attribs = vertexDescriptor.Attributes;
        for (int i = 0; i < description.VertexLayout.Attributes.Length; i++)
        {
            var attrDescr = description.VertexLayout.Attributes[i];
            var attrib = attribs[(int) attrDescr.AttributeIndex];
            attrib.Format = attrDescr.Format;
            attrib.BufferIndex = MetalBufferSlots.VertexIndex(attrDescr.BufferSlot);
            attrib.Offset = attrDescr.Offset;
        }

        var layouts = vertexDescriptor.Layouts;
        for (int i = 0; i < description.VertexLayout.Buffers.Length; i++)
        {
            var bufDescr = description.VertexLayout.Buffers[i];
            var layout = layouts[(int) MetalBufferSlots.VertexIndex(bufDescr.BufferIndex)];
            layout.Stride = bufDescr.Stride;
            layout.StepFunction = bufDescr.StepFunction;
        }
        Pipeline = Device.CreatePipeline(
            vertexDescriptor,
            description.VertexShader.Source,
            description.FragmentShader.Source,
            description.VertexShader.EntryPoint,
            description.FragmentShader.EntryPoint,
            description.Blend
        );
        PrimitiveType = description.PrimitiveType;
        VertexLayout = description.VertexLayout;
    }

    /// <summary>
    /// Disposes of the render pipeline and releases any associated resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Pipeline.Dispose();
        Disposed = true;
    }
}