using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using odl3d;
using odl3d.Renderer;
using odl3d.Renderer.OpenGLAdapter;

namespace odl3ddemo;

public static class Program
{
    public static void Main(string[] args)
    {
        int width = 800;
        int height = 600;

        using Window window = new Window(width, height, "odl3d", RenderTarget.OpenGL);
        window.BackgroundColor = new Color(0, 0, 0);
        window.Camera = new MoveableCamera(window)
        {
            Position = new Vector3(0, 0, 2f)
        };
        window.SetCursorCapture(true);
        window.RegisterKeyPress(Key.Escape, window.Close);
        window.RegisterKeyPress(Key.M, () => window.SetWireFrame(!window.Wireframe));

        using UIScene uiScene = new UIScene(window);
        using DemoScene demoScene = new DemoScene(window);
        using ModelScene modelScene = new ModelScene(window);
        
        while (!window.ShouldClose)
        {
            window.Update(0f);
            window.Render();
        }
    }
}
