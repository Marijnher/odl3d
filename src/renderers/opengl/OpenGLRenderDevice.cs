using System;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLRenderDevice : IRenderDevice
{
    public RenderTarget RenderTarget => RenderTarget.OpenGL;
    public string Name => "OpenGL";

    public IRenderCapabilities Capabilities { get; } = new OpenGLRenderCapabilities();

    public bool Disposed { get; private set; }

    public OpenGLRenderDevice()
    {
        GLFW.glfwWindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 3);
        GLFW.glfwWindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 3);
        GLFW.glfwWindowHint(GLFW.GLFW_OPENGL_PROFILE, GLFW.GLFW_OPENGL_CORE_PROFILE);
        GLFW.glfwWindowHint(GLFW.GLFW_OPENGL_FORWARD_COMPAT, GLFW.GLFW_TRUE);
        // GL is initialized in OpenGLRenderSurface after setting the current context.
    }

    public IRenderSurface CreateSurface(nint nativeWindow) =>
        new OpenGLRenderSurface(nativeWindow);

    public IBuffer<T> CreateBuffer<T>(BufferDescription description, T[]? initialData = null) where T : unmanaged =>
        new OpenGLBuffer<T>(description, initialData);

    public ITexture CreateTexture(TextureDescription description, byte[]? initialData = null) =>
        throw new NotImplementedException();
    
    public ISampler CreateSampler(SamplerDescription? description = null) =>
        new OpenGLSampler(description ?? new SamplerDescription());

    public IDepthStencilState CreateDepthStencilState(DepthStencilDescription description) =>
        new OpenGLDepthStencilState(description);

    public IShaderModule CreateShaderModule(ShaderModuleDescription description) =>
        new OpenGLShaderModule(description);

    public IRenderPipeline CreateRenderPipeline(RenderPipelineDescription description) =>
        new OpenGLRenderPipeline(description);

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
    }
}