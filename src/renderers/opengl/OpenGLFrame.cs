using System;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLFrame : IRenderFrame
{
    private nint WindowHandle;
    public OpenGLRenderSurface RenderSurface { get; private set; }

    public ITexture ColorTexture => throw new NotImplementedException();

    public bool Disposed { get; private set; }

    public GLVertexState VertexState { get; private set; }

    public OpenGLFrame(OpenGLRenderSurface renderSurface, GLVertexState vertexState, nint windowHandle)
    {
        RenderSurface = renderSurface;
        VertexState = vertexState;
        WindowHandle = windowHandle;
    }

    public IRenderPass CreateRenderPass(RenderPassDescription renderPassDescription) =>
        new OpenGLRenderPass(RenderSurface, VertexState, renderPassDescription);
    
    public void Present()
    {
        GLFW.glfwSwapBuffers(WindowHandle);
    }

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
    }
}