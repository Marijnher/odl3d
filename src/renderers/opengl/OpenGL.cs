using System;
using System.Numerics;
using System.Text;
using static odl3d.GL;

namespace odl3d.Renderers;

/// <summary>
/// Provides an OpenGL implementation of the IRenderer interface, which defines methods for creating and managing graphics resources, rendering operations, and shader programs. The OpenGL class encapsulates the functionality of the OpenGL graphics API, allowing for efficient rendering of 3D scenes and objects. It includes methods for initializing the renderer, creating and deleting buffers, textures, and shaders, binding resources, setting data, and performing draw calls. The OpenGL class is responsible for handling the specifics of the OpenGL API while providing a consistent interface for rendering operations.
/// </summary>
public partial class OpenGL : IRenderer
{
    /// <summary>
    /// Initializes the OpenGL renderer, setting up the necessary OpenGL context and state for rendering. This method is called to prepare the renderer for use, ensuring that the OpenGL functions are available and that the rendering context is properly configured. The Initialize method should be called before any rendering operations are performed, allowing the renderer to set up its internal state and resources.
    /// </summary>
    public void Initialize()
    {
        if (!Loaded) Load();
    }

    public void Dispose() { }

    public void SetViewport(int x, int y, int width, int height) => glViewport(x, y, width, height);

    public void SetEnableDepthTest(bool enable)
    {
        if (enable)
            glEnable(GL_DEPTH_TEST);
        else
            glDisable(GL_DEPTH_TEST);
    }

    public void SetDepthMask(bool enable) => glDepthMask(enable ? GL_TRUE : GL_FALSE);

    public void SetAlphaBlending(bool enable)
    {
        if (enable)
            glEnable(GL_BLEND);
        else
            glDisable(GL_BLEND);
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
    }

    public void SetWireFrame(bool enable) => glPolygonMode(GL_FRONT_AND_BACK, enable ? GL_LINE : GL_FILL);

    public void ClearColor(Color color) => glClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    public void ClearColorBuffer() => glClear(GL_COLOR_BUFFER_BIT);

    public void ClearDepthBuffer() => glClear(GL_DEPTH_BUFFER_BIT);

    public void DrawElements(int count) => glDrawElements(GL_TRIANGLES, count, GL_UNSIGNED_INT, IntPtr.Zero);
}