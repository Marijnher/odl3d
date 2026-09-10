using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
namespace odl3d;

/// <summary>
/// Wraps a GLFW window and its OpenGL context; the sole windowing/input entry point for odl3d.
/// </summary>
public class Window : InputHost
{
    /// <summary>
    /// The native handle of the GLFW window. This can be used to set up additional callbacks or query window properties not exposed by this class.
    /// </summary>
    public IntPtr Handle { get; private set; }

    /// <summary>
    /// The current width of the window in pixels.
    /// </summary>
    public int Width { get; private set; }
    
    /// <summary>
    /// The current height of the window in pixels.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Indicates whether the cursor is currently captured (hidden and locked to the window for camera control). When false, the cursor is visible and free to move within the window.
    /// </summary>
    public bool CursorCaptured { get; private set; } = false;

    /// <summary>
    /// The background color used when clearing the window's color buffer. This is set at creation and can be changed at any time. The default is black (0,0,0).
    /// </summary>
    public Color BackgroundColor = new Color(0, 0, 0);

    /// <summary>
    /// The camera used to render 3D scenes in the window. Each 3D scene can have its own camera, but this property provides a default camera for convenience. The camera's view and projection matrices are used to render the objects in the 3D scenes.
    /// </summary>
    public Camera Camera;

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
    /// Indicates whether the window is currently rendering in wireframe mode. When set to true, all geometry is rendered as wireframes instead of filled polygons.
    /// </summary>
    public bool Wireframe { get; private set; } = false;

    /// <summary>
    /// The time at the previous frame, used to calculate delta time between frames. This is updated each frame during the window's update loop.
    /// </summary>
    private double? previousTime;

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
        GLFW.glfwWindowHint(GLFW.GLFW_RESIZABLE, GLFW.GLFW_TRUE);

        Handle = GLFW.glfwCreateWindow(width, height, title, IntPtr.Zero, IntPtr.Zero);
        if (Handle == IntPtr.Zero)
        {
            GLFW.glfwTerminate();
            throw new Exception("Failed to create a GLFW window.");
        }

        GLFW.glfwMakeContextCurrent(Handle);
        GLFW.glfwSwapInterval(0);

        GL.Load();
        GL.glViewport(0, 0, width, height);
        GL.glEnable(GL.GL_DEPTH_TEST);
        GL.glEnable(GL.GL_BLEND);
        GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);

        // Default non-moveable camera
        Camera = new Camera(this);
        Center();
    }

    ~Window()
    {
        if (!Disposed) Console.WriteLine("Warning: Window was not disposed before being finalized. This may cause a GLFW resource leak.");
    }

    /// <summary>
    /// Captures or releases the cursor. When captured, the cursor is hidden and locked to the window, allowing mouse movement to control camera look. When released, the cursor is visible and free to move within the window.
    /// </summary>
    /// <param name="enabled">True to capture the cursor, false to release it.</param>
    public void SetCursorCapture(bool capture)
    {
        GLFW.glfwSetInputMode(Handle, GLFW.GLFW_CURSOR, capture ? GLFW.GLFW_CURSOR_DISABLED : GLFW.GLFW_CURSOR_NORMAL);
        CursorCaptured = capture;
    }

    /// <summary>
    /// Enables or disables input handling for the window. When enabled, the window will create an input manager to handle keyboard and mouse events. When disabled, the input manager will be disposed and input handling will be turned off.
    /// </summary>
    /// <param name="enable">True to enable input handling, false to disable it.</param>
    public override void SetEnableInput(bool enable)
    {
        if (enable && InputManager == null)
        {
            InputManager = new WindowInputManager(this);
        }
        else if (!enable && InputManager != null)
        {
            InputManager.Dispose();
            InputManager = null;
        }
    }

    /// <summary>
    /// Sets the window to render in wireframe mode if enabled is true, or in filled polygon mode if enabled is false.
    /// </summary>
    /// <param name="enabled">True to enable wireframe mode, false to render filled polygons.</param>
    public void SetWireFrame(bool enabled)
    {
        Wireframe = enabled;
        GL.glPolygonMode(GL.GL_FRONT_AND_BACK, enabled ? GL.GL_LINE : GL.GL_FILL);
    }

    /// <summary>
    /// Resizes the window's client area in pixels.
    /// </summary>
    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException();
        GLFW.glfwSetWindowSize(Handle, width, height);
        UpdateWindowSize();
    }

    /// <summary>
    /// Maximizes the window and updates all render dimensions.
    /// </summary>
    public void Maximize()
    {
        GLFW.glfwMaximizeWindow(Handle);
        UpdateWindowSize();
    }

    /// <summary>
    /// Restores the window from its maximized state and updates all render dimensions.
    /// </summary>
    public void Restore()
    {
        GLFW.glfwRestoreWindow(Handle);
        UpdateWindowSize();
    }

    /// <summary>
    /// Centers the window in the primary monitor's usable work area.
    /// </summary>
    public void Center()
    {
        IntPtr monitor = GLFW.glfwGetPrimaryMonitor();
        GLFW.glfwGetMonitorWorkarea(monitor, out int monitorX, out int monitorY, out int monitorWidth, out int monitorHeight);
        GLFW.glfwGetWindowSize(Handle, out int windowWidth, out int windowHeight);
        GLFW.glfwSetWindowPos(Handle,
            monitorX + (monitorWidth - windowWidth) / 2,
            monitorY + (monitorHeight - windowHeight) / 2);
    }

    private void UpdateWindowSize()
    {
        GLFW.glfwGetWindowSize(Handle, out int width, out int height);
        if (width <= 0 || height <= 0) return;
        if (Width == width && Height == height) return;

        Width = width;
        Height = height;
        Camera.AspectRatio = (float) width / height;
        GL.glViewport(0, 0, width, height);
        Scenes2D.ForEach(scene => scene.UpdateWindowSize());
    }

    /// <summary>
    /// Polls for window events, such as input and window close requests. This should be called once per frame before rendering.
    /// </summary>
    public override void Update(float _)
    {
        GLFW.glfwPollEvents();
        UpdateWindowSize();

        double time = GLFW.glfwGetTime();
        float deltaTime = (float) (time - (previousTime ?? time));
        previousTime = time;

        base.Update(deltaTime);
        Scenes3D.ForEach(s => s.Update(deltaTime));
        Scenes2D.ForEach(s => s.Update(deltaTime));
        Camera.Update(deltaTime);
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
    public Vector2 GetCursorPosition()
    {
        GLFW.glfwGetCursorPos(Handle, out double x, out double y);
        return new Vector2((float) x, (float) y);
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
    /// Renders all 3D and 2D scenes in the window using the specified shader. This method clears the window's color and depth buffers, sets the viewport, and then draws each scene in the order they were added. The depth buffer is cleared between the 3D and 2D groups so 2D scenes are always in front of 3D scenes. Within the 3D group, opaque objects across all scenes are drawn before any scene's transparent objects, so transparent objects (e.g. soft shadow decals) always blend against fully-drawn opaque geometry regardless of which scene either belongs to.
    /// </summary>
    /// <param name="shader">The shader to use for rendering the scenes.</param>
    public void Render(Shader shader)
    {
        Clear();
        GL.glViewport(0, 0, Width, Height);
        // Clear depth buffer
        GL.glClear(GL.GL_DEPTH_BUFFER_BIT);
        foreach (Scene3D scene in Scenes3D) scene.DrawPass(shader, RenderPass.Opaque);
        GL.glDepthMask(GL.GL_FALSE);
        foreach (Scene3D scene in Scenes3D) scene.DrawPass(shader, RenderPass.Transparent);
        GL.glDepthMask(GL.GL_TRUE);
        // Clear depth buffer again so all 2D scenes are always in front of 3D scenes
        GL.glClear(GL.GL_DEPTH_BUFFER_BIT);
        foreach (Scene2D scene in Scenes2D)
        {
            scene.Draw(shader);
        }
    }

    /// <summary>
    /// Adds a generic scene to the window. The scene will be rendered in the order it was added, and its type must be either Scene3D or Scene2D. If the scene type is not supported, an ArgumentException is thrown.
    /// </summary>
    /// <typeparam name="T">The type of the scene's objects, which must inherit from Object.</typeparam>
    /// <param name="scene">The generic scene to add to the window.</param>
    /// <exception cref="ArgumentException">Thrown when the scene type is not supported.</exception>
    public void AddScene<T>(Scene<T> scene) where T : Object
    {
        if (scene is Scene3D scene3D)
        {
            Scenes3D.Add(scene3D);
        }
        else if (scene is Scene2D scene2D)
        {
            Scenes2D.Add(scene2D);
        }
        else throw new ArgumentException("The scene type is not supported.");
    }

    /// <summary>
    /// Removes a generic scene from the window. The scene will no longer be rendered after removal.
    /// </summary>
    /// <typeparam name="T">The type of the scene's objects, which must inherit from Object.</typeparam>
    /// <param name="scene">The generic scene to remove from the window.</param>
    /// <exception cref="ArgumentException">Thrown when the scene type is not supported.</exception>
    public void RemoveScene<T>(Scene<T> scene) where T : Object
    {
        if (scene is Scene3D scene3D)
        {
            Scenes3D.Remove(scene3D);
        }
        else if (scene is Scene2D scene2D)
        {
            Scenes2D.Remove(scene2D);
        }
        else throw new ArgumentException("The scene type is not supported.");
    }

    /// <summary>
    /// Disposes of the window and its OpenGL context, releasing any associated resources. After calling this method, the window should not be used again.
    /// </summary>
    public override void Dispose()
    {
        if (Disposed) return;
        while (Scenes3D.Count > 0)
        {
            // Child automatically removes itself from the scene list upon disposal
            Scenes3D[0].Dispose();
        }
        while (Scenes2D.Count > 0)
        {
            // Child automatically removes itself from the scene list upon disposal
            Scenes2D[0].Dispose();
        }
        base.Dispose();
        GLFW.glfwDestroyWindow(Handle);
        GLFW.glfwTerminate();
        Disposed = true;
    }
}
