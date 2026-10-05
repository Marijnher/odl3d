using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL frame used for rendering.
/// </summary>
internal class OpenGLRenderFrame : IRenderFrame
{
    private nint WindowHandle;

    /// <summary>
    /// Gets the OpenGL render device associated with this frame.
    /// </summary>
    public OpenGLRenderDevice Device { get; }

    /// <summary>
    /// Gets the render surface associated with this frame.
    /// </summary>
    public OpenGLRenderSurface RenderSurface { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the frame has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLRenderFrame"/> class.
    /// </summary>
    /// <param name="device">The OpenGL render device associated with this frame.</param>
    /// <param name="renderSurface">The render surface associated with this frame.</param>
    /// <param name="windowHandle">The handle to the window for this frame.</param>
    public OpenGLRenderFrame(OpenGLRenderDevice device, OpenGLRenderSurface renderSurface, nint windowHandle)
    {
        Device = device;
        RenderSurface = renderSurface;
        WindowHandle = windowHandle;
    }

    /// <summary>
    /// Creates a new render pass for this frame.
    /// </summary>
    /// <param name="renderPassDescription">The description of the render pass to create.</param>
    /// <returns>The newly created render pass.</returns>
    public IRenderPass CreateRenderPass(RenderPassDescription renderPassDescription) =>
        Disposed
            ? throw new ObjectDisposedException(nameof(OpenGLRenderFrame))
            : new OpenGLRenderPass(RenderSurface, Device.VertexArrayCache, renderPassDescription);
    
    /// <summary>
    /// Presents the frame by swapping the front and back buffers.
    /// </summary>
    public void Present()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        try
        {
            GLFW.glfwSwapBuffers(WindowHandle);
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Disposes the frame and releases any associated resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
    }
}