using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL frame used for rendering.
/// </summary>
internal class OpenGLRenderFrame : IRenderFrame
{
    private readonly nint _swapWindow;
    private readonly uint _framebuffer;
    private readonly uint _width;
    private readonly uint _height;
    private OpenGLRenderPass? _lastPass;

    /// <summary>
    /// Gets the OpenGL render device associated with this frame.
    /// </summary>
    public OpenGLRenderDevice Device { get; }

    /// <summary>
    /// Gets a value indicating whether the frame has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLRenderFrame"/> class.
    /// </summary>
    /// <param name="device">The OpenGL render device associated with this frame.</param>
    /// <param name="framebuffer">The framebuffer to render into; 0 is the window's default framebuffer.</param>
    /// <param name="width">The width of the render target in pixels.</param>
    /// <param name="height">The height of the render target in pixels.</param>
    /// <param name="swapWindow">The window whose buffers are swapped on present, or 0 for an offscreen frame.</param>
    public OpenGLRenderFrame(OpenGLRenderDevice device, uint framebuffer, uint width, uint height, nint swapWindow)
    {
        Device = device;
        _framebuffer = framebuffer;
        _width = width;
        _height = height;
        _swapWindow = swapWindow;
    }

    /// <summary>
    /// Creates a new render pass for this frame.
    /// </summary>
    /// <param name="renderPassDescription">The description of the render pass to create.</param>
    /// <returns>The newly created render pass.</returns>
    public IRenderPass CreateRenderPass(RenderPassDescription renderPassDescription)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        _lastPass = new OpenGLRenderPass(_framebuffer, _width, _height, Device.VertexArrayCache, renderPassDescription);
        return _lastPass;
    }

    /// <summary>
    /// Reads back the color target of an offscreen frame as RGBA8 pixels with row 0 at the top.
    /// </summary>
    /// <returns>The pixels of the color target.</returns>
    public unsafe byte[] ReadPixels()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        if (_swapWindow != 0)
            throw new RenderException("ReadPixels is only supported on frames of offscreen surfaces.");
        if (_lastPass is { Disposed: false })
            throw new RenderException("All render passes must be ended before reading pixels.");

        int rowBytes = (int) _width * 4;
        byte[] pixels = new byte[rowBytes * (int) _height];
        GL.glBindFramebuffer(GL.GL_READ_FRAMEBUFFER, _framebuffer);
        GL.glPixelStorei(GL.GL_PACK_ALIGNMENT, 1);
        fixed (byte* ptr = pixels)
            GL.glReadPixels(0, 0, (int) _width, (int) _height, (uint) GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, (nint) ptr);
        GL.glBindFramebuffer(GL.GL_READ_FRAMEBUFFER, 0);

        // GL rows start at the bottom; flip so row 0 is the top like on every other backend.
        Span<byte> temp = rowBytes <= 16384 ? stackalloc byte[rowBytes] : new byte[rowBytes];
        for (int top = 0, bottom = (int) _height - 1; top < bottom; top++, bottom--)
        {
            Span<byte> topRow = pixels.AsSpan(top * rowBytes, rowBytes);
            Span<byte> bottomRow = pixels.AsSpan(bottom * rowBytes, rowBytes);
            topRow.CopyTo(temp);
            bottomRow.CopyTo(topRow);
            temp.CopyTo(bottomRow);
        }
        return pixels;
    }
    
    /// <summary>
    /// Presents the frame by swapping the front and back buffers, or flushes the work of an offscreen frame.
    /// </summary>
    public void Present()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        try
        {
            if (_swapWindow != 0) GLFW.glfwSwapBuffers(_swapWindow);
            else GL.glFlush();
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