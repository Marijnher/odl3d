using System;
using System.Linq;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLRenderPipeline : IRenderPipeline
{
    public uint Handle { get; }
    public GLVertexLayout GLLayout { get; }

    public PrimitiveType PrimitiveType { get; }
    public VertexLayoutDescription VertexLayout { get; }
    public BlendDescription Blend { get; }

    public bool Disposed { get; private set; }

    public OpenGLRenderPipeline(RenderPipelineDescription description)
    {
        PrimitiveType = description.PrimitiveType;
        VertexLayout = description.VertexLayout;
        Blend = description.Blend;
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

    public void Use()
    {
        GL.glUseProgram(Handle);
        if (!Blend.Enabled)
        {
            GL.glDisable(GL.GL_BLEND);
            return;
        }
        GL.glEnable(GL.GL_BLEND);
        GL.glBlendFuncSeparate(
            GetBlendFactor(Blend.SourceColor),
            GetBlendFactor(Blend.DestinationColor),
            GetBlendFactor(Blend.SourceAlpha),
            GetBlendFactor(Blend.DestinationAlpha));

        GL.glBlendEquationSeparate(
            GetBlendOperation(Blend.ColorOperation),
            GetBlendOperation(Blend.AlphaOperation));
    }

    static uint GetBlendFactor(BlendFactor f) => f switch
    {
        BlendFactor.Zero                     => GL.GL_ZERO,
        BlendFactor.One                      => GL.GL_ONE,
        BlendFactor.SourceColor              => GL.GL_SRC_COLOR,
        BlendFactor.OneMinusSourceColor      => GL.GL_ONE_MINUS_SRC_COLOR,
        BlendFactor.SourceAlpha              => GL.GL_SRC_ALPHA,
        BlendFactor.OneMinusSourceAlpha      => GL.GL_ONE_MINUS_SRC_ALPHA,
        BlendFactor.DestinationColor         => GL.GL_DST_COLOR,
        BlendFactor.OneMinusDestinationColor => GL.GL_ONE_MINUS_DST_COLOR,
        BlendFactor.DestinationAlpha         => GL.GL_DST_ALPHA,
        BlendFactor.OneMinusDestinationAlpha => GL.GL_ONE_MINUS_DST_ALPHA,
        BlendFactor.BlendColor               => GL.GL_CONSTANT_COLOR,
        BlendFactor.OneMinusBlendColor       => GL.GL_ONE_MINUS_CONSTANT_COLOR,
        BlendFactor.BlendAlpha               => GL.GL_CONSTANT_ALPHA,
        BlendFactor.OneMinusBlendAlpha       => GL.GL_ONE_MINUS_CONSTANT_ALPHA,
        BlendFactor.SourceAlphaSaturated     => GL.GL_SRC_ALPHA_SATURATE,
        BlendFactor.Source1Color             => GL.GL_SRC1_COLOR,
        BlendFactor.OneMinusSource1Color     => GL.GL_ONE_MINUS_SRC1_COLOR,
        BlendFactor.Source1Alpha             => GL.GL_SRC1_ALPHA,
        BlendFactor.OneMinusSource1Alpha     => GL.GL_ONE_MINUS_SRC1_ALPHA,
        _ => throw new RenderException($"Unsupported blend factor: {f}.")
    };

    static uint GetBlendOperation(BlendOperation op) => op switch
    {
        BlendOperation.Add              => GL.GL_FUNC_ADD,
        BlendOperation.Subtract         => GL.GL_FUNC_SUBTRACT,
        BlendOperation.ReverseSubtract  => GL.GL_FUNC_REVERSE_SUBTRACT,
        BlendOperation.Min              => GL.GL_MIN,
        BlendOperation.Max              => GL.GL_MAX,
        _ => throw new RenderException($"Unsupported blend operation: {op}.")
    };

    public void Dispose()
    {
        if (Disposed) return;
        GL.glDeleteProgram(Handle);
        Disposed = true;
    }
}
