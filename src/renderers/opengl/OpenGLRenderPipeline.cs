using System;
using System.Linq;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLRenderPipeline : IRenderPipeline
{
    public uint Handle { get; }
    public GLVertexLayout GLLayout { get; }

    public PrimitiveType PrimitiveType { get; }
    public VertexLayoutDescription VertexLayout { get; }

    public bool Disposed { get; private set; }

    public OpenGLRenderPipeline(RenderPipelineDescription description)
    {
        PrimitiveType = description.PrimitiveType;
        VertexLayout = description.VertexLayout;
        Handle = GL.glCreateProgram();

        var vtxShader = (OpenGLShaderModule) description.VertexShader;
        var fragShader = (OpenGLShaderModule) description.FragmentShader;

        GL.glAttachShader(Handle, vtxShader.Handle);
        GL.glAttachShader(Handle, fragShader.Handle);
        GL.glLinkProgram(Handle);
        GL.glGetProgramiv(Handle, GL.GL_LINK_STATUS, out int linkStatus);
        if (linkStatus == 0)
        {
            var infoLog = new byte[1024];
            GL.glGetProgramInfoLog(Handle, 1024, out int length, infoLog);
            GL.glDeleteProgram(Handle);
            throw new RenderException($"Failed to link program: {System.Text.Encoding.UTF8.GetString(infoLog, 0, length)}");
        }

        // Link the uniform names with indices in the shader program
        foreach (var g in vtxShader.UniformBlocks.Concat(fragShader.UniformBlocks).GroupBy(b => b.Name))
        {
            int slot = g.Select(b => b.Slot).Distinct().Single();   // throws if the stages disagree
            if ((uint) slot >= BufferSlots.MaxUniformBuffers)
                throw new RenderException($"Block {g.Key} uses slot {slot}, out of range.");

            uint index = GL.glGetUniformBlockIndex(Handle, g.Key);
            if (index == GL.GL_INVALID_INDEX) continue;              // optimized out
            GL.glUniformBlockBinding(Handle, index, (uint) slot);
        }

        GLLayout = new GLVertexLayout(VertexLayout);
    }

    public void Dispose()
    {
        if (Disposed) return;
        GL.glDeleteProgram(Handle);
        Disposed = true;
    }
}
