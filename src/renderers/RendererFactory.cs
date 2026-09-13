using System;

namespace odl3d;

public static class RendererFactory
{
    public static IRenderer CreateRenderer(string id) => id switch
    {
        "opengl" => CreateOpenGLRenderer(),
        _ => throw new RenderException($"Unknown renderer id: {id}")
    };

    public static IRenderer CreateOpenGLRenderer()
    {
        return new Renderers.OpenGL();
    }
}