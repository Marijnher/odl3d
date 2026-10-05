using System;
using System.Collections.Generic;
using System.Linq;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// Owns the renderer and native windowing lifetime for a graphics application.
/// </summary>
public sealed class GraphicsApplication : IDisposable
{
    private readonly List<Window> _windows = new();
    private bool _disposed;

    /// <summary>The renderer shared by windows created by this application.</summary>
    public IRenderDevice Renderer { get; }

    /// <summary>Indicates whether this application and its renderer have been disposed.</summary>
    public bool Disposed => _disposed;

    /// <summary>Creates an application and initializes its renderer backend.</summary>
    /// <param name="renderTarget">The renderer backend to use, or null for the platform default.</param>
    public GraphicsApplication(RenderTarget? renderTarget = null)
    {
        GLFW.Load();
        try
        {
            Renderer = RenderFactory.Create(renderTarget);
        }
        catch
        {
            GLFW.Terminate();
            throw;
        }
    }

    /// <summary>Creates a window and render surface owned by this application.</summary>
    /// <exception cref="NotSupportedException">The OpenGL backend currently supports only one window per application.</exception>
    public Window CreateWindow(int width, int height, string title)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (Renderer.RenderTarget == RenderTarget.OpenGL && _windows.Count > 0)
            throw new NotSupportedException("An OpenGL GraphicsApplication currently supports one window.");
        return new Window(this, width, height, title);
    }

    internal void RegisterWindow(Window window)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _windows.Add(window);
    }

    internal void RemoveWindow(Window window) => _windows.Remove(window);

    public void Dispose()
    {
        if (_disposed) return;
        foreach (Window window in _windows.ToArray())
            window.Dispose();
        Renderer.Dispose();
        GLFW.Terminate();
        _disposed = true;
    }
}