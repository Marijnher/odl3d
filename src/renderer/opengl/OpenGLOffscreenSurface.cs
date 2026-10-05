using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL render surface backed by a framebuffer object instead of a window.
/// </summary>
internal class OpenGLOffscreenSurface : IRenderSurface
{
    private readonly OpenGLRenderDevice _device;
    private readonly nint _ownedContextWindow;
    private uint _framebuffer;
    private uint _colorRenderbuffer;
    private uint _depthRenderbuffer;

    /// <summary>
    /// Gets the width of the render surface in pixels.
    /// </summary>
    public uint Width { get; private set; }

    /// <summary>
    /// Gets the height of the render surface in pixels.
    /// </summary>
    public uint Height { get; private set; }

    /// <summary>
    /// Gets or sets the VSync flag. Offscreen surfaces never wait for the display, so this has no effect.
    /// </summary>
    public bool VSync { get; set; }

    /// <summary>
    /// Gets the depth format of the render surface.
    /// </summary>
    public TextureFormat? DepthFormat => TextureFormat.Depth32Float;

    /// <summary>
    /// Gets the sample count of the render surface.
    /// </summary>
    public int SampleCount => 1;

    /// <summary>
    /// Gets a value indicating whether the render surface has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLOffscreenSurface"/> class. Uses the current OpenGL
    /// context if there is one, so resources are shared with an existing window; otherwise a hidden context is created.
    /// </summary>
    /// <param name="device">The OpenGL render device associated with the render surface.</param>
    /// <param name="width">The width of the surface in pixels.</param>
    /// <param name="height">The height of the surface in pixels.</param>
    public OpenGLOffscreenSurface(OpenGLRenderDevice device, uint width, uint height)
    {
        ValidateSize(width, height);
        _device = device;

        if (GLFW.glfwGetCurrentContext() == IntPtr.Zero)
        {
            GLFW.glfwWindowHint(GLFW.GLFW_VISIBLE, GLFW.GLFW_FALSE);
            _ownedContextWindow = GLFW.glfwCreateWindow(1, 1, "odl3d offscreen", IntPtr.Zero, IntPtr.Zero);
            GLFW.glfwWindowHint(GLFW.GLFW_VISIBLE, GLFW.GLFW_TRUE);
            if (_ownedContextWindow == IntPtr.Zero)
                throw new RenderException("Failed to create a hidden OpenGL context for the offscreen surface.");
            GLFW.glfwMakeContextCurrent(_ownedContextWindow);
        }

        try
        {
            GL.Load();
            _device.InitializeCapabilities();
            CreateAttachments(width, height);
        }
        catch
        {
            DestroyOwnedContext();
            throw;
        }
    }

    /// <summary>
    /// Resizes the render surface, discarding its current contents.
    /// </summary>
    /// <param name="width">The new width of the render surface in pixels.</param>
    /// <param name="height">The new height of the render surface in pixels.</param>
    public void Resize(int width, int height)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        MakeOwnedContextCurrent();
        DeleteAttachments();
        CreateAttachments((uint) width, (uint) height);
    }

    /// <summary>
    /// Acquires a new render frame that renders into the offscreen framebuffer.
    /// </summary>
    /// <returns>The acquired render frame.</returns>
    public IRenderFrame? AcquireFrame()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        MakeOwnedContextCurrent();
        return new OpenGLRenderFrame(_device, _framebuffer, Width, Height, swapWindow: 0);
    }

    private static void ValidateSize(uint width, uint height)
    {
        if (width == 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height == 0) throw new ArgumentOutOfRangeException(nameof(height));
    }

    private void MakeOwnedContextCurrent()
    {
        if (_ownedContextWindow != IntPtr.Zero && GLFW.glfwGetCurrentContext() != _ownedContextWindow)
            GLFW.glfwMakeContextCurrent(_ownedContextWindow);
    }

    private void CreateAttachments(uint width, uint height)
    {
        GL.glGenRenderbuffers(1, out _colorRenderbuffer);
        GL.glBindRenderbuffer(GL.GL_RENDERBUFFER, _colorRenderbuffer);
        GL.glRenderbufferStorage(GL.GL_RENDERBUFFER, (uint) GL.GL_RGBA8, (int) width, (int) height);

        GL.glGenRenderbuffers(1, out _depthRenderbuffer);
        GL.glBindRenderbuffer(GL.GL_RENDERBUFFER, _depthRenderbuffer);
        GL.glRenderbufferStorage(GL.GL_RENDERBUFFER, (uint) GL.GL_DEPTH_COMPONENT32F, (int) width, (int) height);
        GL.glBindRenderbuffer(GL.GL_RENDERBUFFER, 0);

        GL.glGenFramebuffers(1, out _framebuffer);
        GL.glBindFramebuffer(GL.GL_FRAMEBUFFER, _framebuffer);
        GL.glFramebufferRenderbuffer(GL.GL_FRAMEBUFFER, GL.GL_COLOR_ATTACHMENT0, GL.GL_RENDERBUFFER, _colorRenderbuffer);
        GL.glFramebufferRenderbuffer(GL.GL_FRAMEBUFFER, GL.GL_DEPTH_ATTACHMENT, GL.GL_RENDERBUFFER, _depthRenderbuffer);
        uint status = GL.glCheckFramebufferStatus(GL.GL_FRAMEBUFFER);
        GL.glBindFramebuffer(GL.GL_FRAMEBUFFER, 0);

        if (status != GL.GL_FRAMEBUFFER_COMPLETE)
        {
            DeleteAttachments();
            throw new RenderException($"The offscreen framebuffer is incomplete (status 0x{status:X}).");
        }

        Width = width;
        Height = height;
    }

    private void DeleteAttachments()
    {
        if (_framebuffer != 0) GL.glDeleteFramebuffers(1, ref _framebuffer);
        if (_colorRenderbuffer != 0) GL.glDeleteRenderbuffers(1, ref _colorRenderbuffer);
        if (_depthRenderbuffer != 0) GL.glDeleteRenderbuffers(1, ref _depthRenderbuffer);
        _framebuffer = _colorRenderbuffer = _depthRenderbuffer = 0;
    }

    private void DestroyOwnedContext()
    {
        if (_ownedContextWindow == IntPtr.Zero) return;
        // Cached VAOs belong to the context that is about to be destroyed.
        if (GL.Loaded) _device.VertexArrayCache.Clear();
        GLFW.glfwDestroyWindow(_ownedContextWindow);
    }

    /// <summary>
    /// Disposes the render surface, its framebuffer and, if it created one, its hidden context.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        MakeOwnedContextCurrent();
        DeleteAttachments();
        DestroyOwnedContext();
        Disposed = true;
    }
}
