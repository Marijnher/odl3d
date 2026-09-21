using System;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLRenderSurface : IRenderSurface
{
    private nint WindowHandle;
    public GLVertexState VertexState { get; private set; }

    public uint Width { get; }
    public uint Height { get; }
    private bool _vsync;
    public bool VSync
    {
        get => _vsync;
        set
        {
            _vsync = value;
            GLFW.glfwSwapInterval(value ? 1 : 0);
        }
    }
    
    public TextureFormat ColorFormat => throw new NotImplementedException();
    public TextureFormat? DepthFormat => throw new NotImplementedException();
    public int SampleCount => throw new NotImplementedException();

    public bool Disposed { get; private set; }

    public OpenGLRenderSurface(nint windowHandle)
    {
        WindowHandle = windowHandle;
        GLFW.glfwMakeContextCurrent(WindowHandle);
        GL.Load();
        GLFW.glfwGetFramebufferSize(WindowHandle, out int fbWidth, out int fbHeight);
        Width = (uint) fbWidth;
        Height = (uint) fbHeight;
        VSync = true;
        VertexState = new GLVertexState();
    }

    public void Resize(int width, int height)
    {
        // OpenGL automatically handles framebuffer scaling, so we just set the window size.
        GLFW.glfwSetWindowSize(WindowHandle, width, height);
    }
    
    public IRenderFrame? AcquireFrame() =>
        new OpenGLFrame(this, VertexState, WindowHandle);

    public void Dispose()
    {
        if (Disposed) return;
        GLFW.glfwMakeContextCurrent(IntPtr.Zero);
        Disposed = true;
    }
}