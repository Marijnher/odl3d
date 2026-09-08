using System;
using System.Collections.Generic;
using System.Linq;

namespace odl3d;

/// <summary>
/// Wraps a GLFW window and its OpenGL context; the sole windowing/input entry point for odl3d.
/// </summary>
public class Window : IDisposable
{
    /// <summary>
    /// The native handle of the GLFW window. This can be used to set up additional callbacks or query window properties not exposed by this class.
    /// </summary>
    public IntPtr Handle { get; private set; }

    /// <summary>
    /// The width of the window in pixels. This is set at creation and does not change, as the window is non-resizable.
    /// </summary>
    public int Width { get; private set; }
    
    /// <summary>
    /// The height of the window in pixels. This is set at creation and does not change, as the window is non-resizable.
    /// </summary>
    public int Height { get; private set; }
    
    /// <summary>
    /// Indicates whether the window has been disposed and its resources released. After disposing, the window should not be used again.
    /// </summary>
    public bool Disposed { get; private set; } = false;

    /// <summary>
    /// The background color used when clearing the window's color buffer. This is set at creation and can be changed at any time. The default is black (0,0,0).
    /// </summary>
    public Color BackgroundColor { get; set; } = new Color(0, 0, 0);

    /// <summary>
    /// The camera used to render 3D scenes in the window. Each 3D scene can have its own camera, but this property provides a default camera for convenience. The camera's view and projection matrices are used to render the objects in the 3D scenes.
    /// </summary>
    public Camera Camera { get; set; }

    /// <summary>
    /// A window can display any number of 3D scenes, layered in the order added. Each scene's camera is used to render its objects, and the depth buffer is cleared between scenes so later scenes are not occluded by earlier ones.
    /// </summary>
    public List<Scene3D> Scenes3D { get; } = new List<Scene3D>();

    /// <summary>
    /// A window can display any number of 2D scenes, layered in the order added. Each scene's viewport is defined in pixel coordinates relative to the top-left of the window.
    /// </summary>
    public List<Scene2D> Scenes2D { get; } = new List<Scene2D>();

    /// <summary>
    /// Indicates whether the window has been marked to close (e.g. by pressing Escape). This does not immediately destroy the window; it is up to the application to check this property and call Dispose() when appropriate.
    /// </summary>
    public bool ShouldClose => GLFW.glfwWindowShouldClose(Handle) != GLFW.GLFW_FALSE;

    /// <summary>
    /// The InputManager instance that handles input events for this window. It provides methods to query the state of keys, mouse buttons, and cursor position, and can be used to implement movement controls, camera look, and other interactive features.
    /// </summary>
    public WindowInputManager InputManager { get; private set; }

    /// <summary>
    /// Creates a new window with the specified width, height, and title. The window is centered on the primary monitor and its OpenGL context is made current. The cursor is hidden and locked to the window so mouse movement can drive camera look.
    /// <param name="width">Window width in pixels.</param>
    /// <param name="height">Window height in pixels.</param>
    /// <param name="title">Window title.</param>
    /// </summary>
    public Window(int width, int height, string title)
    {
        Width = width;
        Height = height;

        GLFW.Load();
        if (GLFW.glfwInit() == GLFW.GLFW_FALSE)
            throw new Exception("Failed to initialize GLFW.");

        GLFW.glfwWindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 3);
        GLFW.glfwWindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 3);
        GLFW.glfwWindowHint(GLFW.GLFW_OPENGL_PROFILE, GLFW.GLFW_OPENGL_CORE_PROFILE);
        GLFW.glfwWindowHint(GLFW.GLFW_OPENGL_FORWARD_COMPAT, GLFW.GLFW_TRUE);
        GLFW.glfwWindowHint(GLFW.GLFW_RESIZABLE, GLFW.GLFW_FALSE);

        Handle = GLFW.glfwCreateWindow(width, height, title, IntPtr.Zero, IntPtr.Zero);
        if (Handle == IntPtr.Zero)
        {
            GLFW.glfwTerminate();
            throw new Exception("Failed to create a GLFW window.");
        }

        GLFW.glfwMakeContextCurrent(Handle);
        GLFW.glfwSwapInterval(1);

        GL.Load();
        GL.glViewport(0, 0, width, height);
        GL.glEnable(GL.GL_DEPTH_TEST);

        // Hides and locks the cursor to the window so mouse movement can drive camera look.
        GLFW.glfwSetInputMode(Handle, GLFW.GLFW_CURSOR, GLFW.GLFW_CURSOR_DISABLED);

        Camera = new Camera(width, height);
        InputManager = new WindowInputManager(this);
    }

    ~Window()
    {
        if (!Disposed) Console.WriteLine("Warning: Window was not disposed before being finalized. This may cause a GLFW resource leak.");
    }

    /// <summary>
    /// Polls for window events, such as input and window close requests. This should be called once per frame before rendering.
    /// </summary>
    public void PollEvents()
    {
        GLFW.glfwPollEvents();
        InputManager.Update();
    }

    /// <summary>
    /// Swaps the front and back buffers, displaying the rendered scene to the window. This should be called after Render().
    /// </summary>
    public void SwapBuffers() => GLFW.glfwSwapBuffers(Handle);

    /// <summary>
    /// Marks the window to close, which will cause ShouldClose to return true. The window is not immediately destroyed; it is up to the application to check ShouldClose and call Dispose() when appropriate.
    /// </summary>
    public void Close() => GLFW.glfwSetWindowShouldClose(Handle, GLFW.GLFW_TRUE);

    /// <summary>
    /// Returns the current cursor position in window pixel coordinates, with (0,0) at the top-left of the window. This can be used to implement camera look controls, for example.
    /// </summary>
    /// <returns>A tuple containing the X and Y coordinates of the cursor.</returns>
    public (double X, double Y) GetCursorPosition()
    {
        GLFW.glfwGetCursorPos(Handle, out double x, out double y);
        return (x, y);
    }

    /// <summary>
    /// Returns the current time in seconds since GLFW was initialized. This can be used to implement frame timing and movement speed controls, for example.
    /// </summary>
    /// <returns>The current time in seconds since GLFW was initialized.</returns>
    public static double GetTime() => GLFW.glfwGetTime();

    /// <summary>
    /// Clears the window's color and depth buffers using the BackgroundColor property. This should be called at the start of each frame before rendering any scenes.
    /// </summary>
    public void Clear()
    {
        GL.glClearColor(BackgroundColor.R / 255f, BackgroundColor.G / 255f, BackgroundColor.B / 255f, BackgroundColor.A / 255f);
        GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
    }

    /// <summary>
    /// Renders all 3D and 2D scenes in the order they were added. Each scene's camera is used to render its objects, and the depth buffer is cleared between scenes so later scenes are not occluded by earlier ones. This should be called once per frame after PollEvents() and before SwapBuffers().
    /// </summary>
    /// <param name="shader">The shader to use for rendering the scenes.</param>
    public void Render(Shader shader)
    {
        Clear();
        GL.glViewport(0, 0, Width, Height);
        foreach (Scene3D scene in Scenes3D)
        {
            scene.Draw(shader);
            GL.glClear(GL.GL_DEPTH_BUFFER_BIT); // keep later scenes from being occluded by earlier ones
        }
        foreach (Scene2D scene in Scenes2D)
        {
            scene.Draw(shader);
            GL.glClear(GL.GL_DEPTH_BUFFER_BIT); // keep later scenes from being occluded by earlier ones
        }
    }

    /// <summary>
    /// Adds a 3D scene to the window. The scene will be rendered in the order it was added, and its camera will be used to render its objects. The depth buffer is cleared between scenes so later scenes are not occluded by earlier ones.
    /// </summary>
    /// <param name="scene">The 3D scene to add to the window.</param>
    public void AddScene(Scene3D scene)
    {
        Scenes3D.Add(scene);
    }

    /// <summary>
    /// Adds a 2D scene to the window. The scene will be rendered in the order it was added, and its viewport will be used to position its sprites in pixel coordinates relative to the top-left of the window. The depth buffer is cleared between scenes so later scenes are not occluded by earlier ones.
    /// </summary>
    /// <param name="scene">The 2D scene to add to the window.</param>
    public void AddScene(Scene2D scene)
    {
        Scenes2D.Add(scene);
    }

    /// <summary>
    /// Disposes of the window and its OpenGL context, releasing any associated resources. After calling this method, the window should not be used again.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        while (Scenes3D.Count > 0)
        {
            Scenes3D[0].Dispose();
            Scenes3D.RemoveAt(0);
        }
        while (Scenes2D.Count > 0)
        {
            Scenes2D[0].Dispose();
            Scenes2D.RemoveAt(0);
        }
        InputManager.Dispose();
        GLFW.glfwDestroyWindow(Handle);
        GLFW.glfwTerminate();
        Disposed = true;
    }
}
