using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using odl3d;
using odl3d.Renderer;
using odl3d.Renderer.MetalAdapter;

namespace odl3ddemo;

public static class Program
{
    public static void Main(string[] args)
    {
        int width = 800;
        int height = 600;

        GLFW.Load();
        
        using Window window = new Window(width, height, "odl3d");
        window.BackgroundColor = new Color(0, 0, 0);
        window.Camera = new MoveableCamera(window)
        {
            Position = new Vector3(0, 0, 2f)
        };
        window.SetCursorCapture(true);
        window.RegisterKeyPress(Key.Escape, window.Close);
        window.RegisterKeyPress(Key.M, () => window.SetWireFrame(!window.Wireframe));

        UIScene uiScene = new UIScene(window);
        DemoScene demoScene = new DemoScene(window);
        ModelScene modelScene = new ModelScene(window);
        
        while (!window.ShouldClose)
        {
            window.Update(0f);
            window.Render();
        }

        return;

        // DemoScene demoScene1 = new DemoScene(window);
        // UIScene uiScene = new UIScene(window);
        // ModelScene daeScene = new ModelScene(window);

        // while (!window.ShouldClose)
        // {
        //     window.Update(0);
        //     window.Render(shader);
        //     window.SwapBuffers();
        // }

        // window.Dispose();
        //shader.Dispose();
    }
}
