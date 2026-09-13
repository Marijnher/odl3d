using System;

namespace odl3d;

public static class RenderFactory
{
    private static IRenderer? _instance;
    public static IRenderer Renderer => _instance ?? throw new RenderException("Renderer has not been initialized. Call RenderFactory.Create() first.");

    public static IRenderer Create(string id) => _instance = id switch
    {
        "opengl" => CreateOpenGLRenderer(),
        _ => throw new RenderException($"Unknown renderer id: {id}")
    };

    private static IRenderer CreateOpenGLRenderer()
    {
        return new Renderers.OpenGL();
    }
}