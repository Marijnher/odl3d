using System;
using System.Numerics;
using System.Text;
using static odl3d.GL;

namespace odl3d.Renderers;

public partial class OpenGL : IRenderer
{
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