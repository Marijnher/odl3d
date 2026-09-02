using System;
using System.Drawing;
using System.Numerics;

namespace odl3d;

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
    FragColor = uUseTexture ? texture(uTexture, vTexCoord) * texColor : uColor;
}";

    public static void Main()
    {
        int width = 800;
        int height = 600;
        Window window = new Window(width, height, "odl3d");
        window.BackgroundColor = new Color(0, 0, 0);

        Shader shader = new Shader(VertexSource, FragmentSource);

        DemoScene demoScene1 = new DemoScene(window);
        UIScene uiScene = new UIScene(window);

        double lastTime = Window.GetTime();
        var (lastMouseX, lastMouseY) = window.GetCursorPosition();
        const float moveSpeed = 3f;
        const float mouseSensitivity = 0.1f;

        while (!window.ShouldClose)
        {
            window.PollEvents();

            if (window.IsKeyPressed(GLFW.GLFW_KEY_ESCAPE)) window.Close();

            double time = Window.GetTime();
            float deltaTime = (float) (time - lastTime);
            lastTime = time;

            Camera camera = window.Camera;

            Vector3 movement = Vector3.Zero;
            if (window.IsKeyPressed(GLFW.GLFW_KEY_W)) movement += camera.Front;
            if (window.IsKeyPressed(GLFW.GLFW_KEY_S)) movement += camera.Back;
            if (window.IsKeyPressed(GLFW.GLFW_KEY_A)) movement += camera.Left;
            if (window.IsKeyPressed(GLFW.GLFW_KEY_D)) movement += camera.Right;
            if (window.IsKeyPressed(GLFW.GLFW_KEY_SPACE)) movement += camera.Up;
            if (window.IsKeyPressed(GLFW.GLFW_KEY_LEFT_SHIFT)) movement += camera.Down;
            if (movement != Vector3.Zero)
                camera.Position += Vector3.Normalize(movement) * moveSpeed * deltaTime;

            var (mouseX, mouseY) = window.GetCursorPosition();
            double deltaX = mouseX - lastMouseX;
            double deltaY = mouseY - lastMouseY;
            lastMouseX = mouseX;
            lastMouseY = mouseY;
            // screen Y grows downward, so an upward mouse move should increase pitch
            camera.Rotate((float) deltaX * mouseSensitivity, (float) -deltaY * mouseSensitivity);

            window.Render(shader);
            window.SwapBuffers();
        }

        window.Dispose();
        shader.Dispose();
        Mesh.DisposeShared();
    }
}
