using System;
using System.Numerics;
using odl3d;

namespace odl3d.Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        using GraphicsApplication app = new GraphicsApplication();
        using Window window = app.CreateWindow(800, 600, "odl3d");
        window.BackgroundColor = new Color(0, 0, 0);
        window.Camera = new MoveableCamera(window)
        {
            Position = new Vector3(0, 0, 2f)
        };
        window.CursorCapture = true;
        window.RegisterKeyPress(Key.Escape, window.Close);
        window.RegisterKeyPress(Key.M, () => window.Wireframe = !window.Wireframe);

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
