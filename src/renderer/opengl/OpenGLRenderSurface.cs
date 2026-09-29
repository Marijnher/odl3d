using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL render surface that manages the window context and framebuffer.
/// </summary>
internal class OpenGLRenderSurface : IRenderSurface
{
    private nint WindowHandle;

    /// <summary>
    /// The OpenGL render device associated with this render surface.
    /// </summary>
    public OpenGLRenderDevice Device;

    /// <summary>
    /// The width of the render surface in pixels.
    /// </summary>
    public uint Width { get; }
    
    /// <summary>
    /// The height of the render surface in pixels.
    /// </summary>
    public uint Height { get; }

    private bool _vsync;
    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization (VSync) is enabled for the render surface.
    /// </summary>
    public bool VSync
    {
        get => _vsync;
        set
        {
            _vsync = value;
            GLFW.glfwSwapInterval(value ? 1 : 0);
        }
    }
    
    /// <summary>
    /// Gets the color format of the render surface.
    /// </summary>
    public TextureFormat ColorFormat => throw new NotImplementedException();

    /// <summary>
    /// Gets the depth format of the render surface, if any.
    /// </summary>
    public TextureFormat? DepthFormat => throw new NotImplementedException();

    /// <summary>
    /// Gets the sample count (number of multisample anti-aliasing samples) of the render surface.
    /// </summary>
    public int SampleCount => throw new NotImplementedException();

    /// <summary>
    /// Gets a value indicating whether the render surface has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLRenderSurface"/> class with the specified render device and window handle.
    /// </summary>
    /// <param name="device">The OpenGL render device associated with the render surface.</param>
    /// <param name="windowHandle">The handle to the window for the render surface.</param>
    public OpenGLRenderSurface(OpenGLRenderDevice device, nint windowHandle)
    {
        Device = device;
        WindowHandle = windowHandle;
        GLFW.glfwMakeContextCurrent(WindowHandle);
        GL.Load();
        GLFW.glfwGetFramebufferSize(WindowHandle, out int fbWidth, out int fbHeight);
        Width = (uint) fbWidth;
        Height = (uint) fbHeight;
        VSync = true;
        Device.VertexState = new GLVertexState();
    }

    /// <summary>
    /// Resizes the render surface to the specified width and height in pixels.
    /// </summary>
    /// <param name="width">The new width of the render surface in pixels.</param>
    /// <param name="height">The new height of the render surface in pixels.</param>
    public void Resize(int width, int height)
    {
        // OpenGL automatically handles framebuffer scaling, so we just set the window size.
        GLFW.glfwSetWindowSize(WindowHandle, width, height);
    }
    
    /// <summary>
    /// Acquires a new render frame for the render surface.
    /// </summary>
    /// <returns>The acquired render frame, or null if no frame could be acquired.</returns>
    public IRenderFrame? AcquireFrame() =>
        new OpenGLFrame(Device, this, WindowHandle);

    /// <summary>
    /// Disposes the render surface and releases any associated resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        GLFW.glfwMakeContextCurrent(IntPtr.Zero);
        Disposed = true;
    }
}