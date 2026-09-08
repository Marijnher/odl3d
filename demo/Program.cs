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
    FragColor = uUseTexture ? texture(uTexture, vTexCoord) * texColor : uColor;
}";

    public static void Main()
    {
        int width = 800;
        int height = 600;
        Window window = new Window(width, height, "odl3d");
        window.BackgroundColor = new Color(0, 0, 0);
        window.SetCursor(true);
        window.RegisterKeyPress(Key.Escape, () => window.Close());

        Shader shader = new Shader(VertexSource, FragmentSource);

        DemoScene demoScene1 = new DemoScene(window);
        UIScene uiScene = new UIScene(window);
        
        demoScene1.SetEnableInput(true);
        demoScene1.RegisterKeyPress(Key.C, () => Console.WriteLine("C pressed."));
        demoScene1.RegisterKeyReleased(Key.C, () => Console.WriteLine("C released."));
        demoScene1.RegisterKeyDown(Key.C, () => Console.WriteLine("C down."));

        demoScene1.RegisterMousePress(Mouse.Left, () => Console.WriteLine("Left pressed."));
        demoScene1.RegisterMouseRelease(Mouse.Left, () => Console.WriteLine("Left released."));
        demoScene1.RegisterMouseDown(Mouse.Left, () => Console.WriteLine("Left down."));
        demoScene1.RegisterMouseRepeated(Mouse.Right, () => Console.WriteLine("Right repeated."));

        demoScene1.RegisterKeyRepeated(Key.V, () => Console.WriteLine("V repeated."));

        double lastTime = Window.GetTime();
        var (lastMouseX, lastMouseY) = window.GetCursorPosition();
        const float moveSpeed = 3f;
        const float mouseSensitivity = 0.1f;

        while (!window.ShouldClose)
        {
            window.Update();

            double time = Window.GetTime();
            float deltaTime = (float) (time - lastTime);
            lastTime = time;

            Camera camera = window.Camera;

            Vector3 movement = Vector3.Zero;
            if (window.IsKeyDown(Key.W)) movement += camera.Front;
            if (window.IsKeyDown(Key.S)) movement += camera.Back;
            if (window.IsKeyDown(Key.A)) movement += camera.Left;
            if (window.IsKeyDown(Key.D)) movement += camera.Right;
            if (window.IsKeyDown(Key.Space)) movement += camera.Up;
            if (window.IsKeyDown(Key.LeftShift)) movement += camera.Down;
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
