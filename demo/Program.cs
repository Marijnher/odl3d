using System;
using odl3d;

namespace odl3ddemo;

public static class Program
{
    private const string VertexSource = @"#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;
out vec2 vTexCoord;
out vec3 vNormal;
uniform mat4 uMVP;
uniform mat4 uModel;
void main()
{
    gl_Position = uMVP * vec4(aPos, 1.0);
    vTexCoord = aTexCoord;
    vNormal = mat3(uModel) * aNormal;
}";

    private const string FragmentSource = @"#version 330 core
in vec2 vTexCoord;
in vec3 vNormal;
out vec4 FragColor;
uniform sampler2D uTexture;
uniform bool uUseTexture;
uniform bool uLit;
uniform vec4 uColor;
uniform vec4 texColor;
void main()
{
    vec4 color = uUseTexture ? texture(uTexture, vTexCoord) * texColor : uColor;

    // Discard fully transparent fragments
    if (color.a == 0)
        discard;

    // Meshes with normals (e.g. extruded 3D text) get simple two-sided directional lighting.
    if (uLit)
    {
        vec3 normal = normalize(vNormal);
        vec3 lightDirection = normalize(vec3(-0.35, 0.6, 1.0));
        float diffuse = max(abs(dot(normal, lightDirection)), 0.0);
        color.rgb *= 0.35 + 0.65 * diffuse;
    }

    FragColor = color;
}";

    public static void Main(string[] args)
    {
        int width = 800;
        int height = 600;

        GLFW.Load();
        RenderFactory.Create("opengl");
        
        Window window = new Window(width, height, "odl3d");
        window.BackgroundColor = new Color(0, 0, 0);
        window.Camera = new MoveableCamera(window);
        window.SetCursorCapture(true);
        window.RegisterKeyPress(Key.Escape, window.Close);
        window.RegisterKeyPress(Key.M, () => window.SetWireFrame(!window.Wireframe));

        ShaderProgram shader = new ShaderProgram(VertexSource, FragmentSource);

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
    }
}
