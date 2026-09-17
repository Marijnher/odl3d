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

        using IRenderDevice renderer = new MetalRenderDevice();
        Console.WriteLine($"Renderer: {renderer.Name}");
        
        using Window window = new Window(renderer, width, height, "odl3d");
        window.BackgroundColor = new Color(0, 0, 0);
        window.Camera = new MoveableCamera(window);
        window.SetCursorCapture(true);
        window.RegisterKeyPress(Key.Escape, window.Close);
        window.RegisterKeyPress(Key.M, () => window.SetWireFrame(!window.Wireframe));

        while (!window.ShouldClose)
        {
            GLFW.glfwPollEvents();
        }

        return;

        // bool metal = RenderFactory.Renderer is odl3d.Renderer.MetalOld;
        // ShaderProgram shader = new ShaderProgram(
        //     metal ? MetalVertexSource : VertexSource,
        //     metal ? MetalFragmentSource : FragmentSource);

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
