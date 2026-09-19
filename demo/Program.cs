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

        Scene3D scene = new Scene3D(window);

        //Mesh mesh = MeshBuilder.CreateSphere(1);
        Mesh mesh = MeshBuilder.CreatePlane(1, 1, 0.1f);
        Texture grassTexture = new Texture("assets/grass.png");
        Object3D obj = new Object3D(scene, mesh, null);

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
