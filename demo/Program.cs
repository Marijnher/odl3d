using System;
using System.Drawing;
using System.Numerics;

namespace odl3d.Demo;

public static class Program
{
    private const string VertexSource = @"#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
out vec2 vTexCoord;
uniform mat4 uMVP;
void main()
{
    gl_Position = uMVP * vec4(aPos, 1.0);
    vTexCoord = aTexCoord;
}";

    private const string FragmentSource = @"#version 330 core
in vec2 vTexCoord;
out vec4 FragColor;
uniform sampler2D uTexture;
uniform bool uUseTexture;
uniform vec4 uColor;
uniform vec4 texColor;
void main()
{
    vec4 color = uUseTexture ? texture(uTexture, vTexCoord) * texColor : uColor;

    // Discard fully transparent fragments
    if (color.a == 0)
        discard;

    FragColor = color;
}";

    public static void Main()
    {
        int width = 800;
        int height = 600;
        Window window = new Window(width, height, "odl3d");
        window.Maximize();
        window.BackgroundColor = new Color(0, 0, 0);
        window.Camera = new MoveableCamera(window);
        window.SetCursorCapture(true);
        window.RegisterKeyPress(Key.Escape, window.Close);
        window.RegisterKeyPress(Key.M, () => window.SetWireFrame(!window.Wireframe));

        Shader shader = new Shader(VertexSource, FragmentSource);

        DemoScene demoScene1 = new DemoScene(window);
        UIScene uiScene = new UIScene(window);
        ModelScene daeScene = new ModelScene(window);

        while (!window.ShouldClose)
        {
            window.Update(0);
            window.Render(shader);
            window.SwapBuffers();
        }

        window.Dispose();
        shader.Dispose();
        Mesh.DisposeShared();
    }
}
