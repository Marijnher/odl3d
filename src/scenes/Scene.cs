using System.Collections.Generic;
using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// A generic scene containing a collection of objects of type T, where T is constrained to be a subclass of Object. The scene provides methods to add and remove objects, and an abstract Draw method that must be implemented by subclasses to render the scene using a given shader.
/// </summary>
/// <typeparam name="T">The type of objects contained in the scene, constrained to be a subclass of Object.</typeparam>
public abstract class Scene<T> : IDisposable where T : Object
{
    /// <summary>
    /// The window associated with the scene, used to determine the rendering context and other properties. This property is set in the constructor and is read-only for subclasses. The window provides access to the OpenGL context, input handling, and other features necessary for rendering the scene's objects.
    /// </summary>
    protected Window Window { get; }

    /// <summary>
    /// The camera used to render the scene. The camera's view and projection matrices are combined to create the view-projection matrix used for rendering the objects in the scene. The camera can be configured with position, orientation, field of view, aspect ratio, and other properties to control how the scene is viewed. This property is read-only for subclasses and is derived from the associated window.
    /// </summary>
    protected Camera Camera => Window.Camera;

    /// <summary>
    /// An optional offset applied to the positions of all objects in the scene when calculating their model matrices. This offset can be used to shift the entire scene in world space without modifying the individual positions of the objects. The offset is added to each object's position when computing its model matrix, allowing for easy translation of the entire scene.
    /// This is intended for Scene3D. For Scene2D, you should prefer to the Viewport property instead.
    /// </summary>
    public Vector3 SceneOffset = Vector3.Zero;

    /// <summary>
    /// Initializes a new instance of the Scene class with the specified window. The window is used to determine the rendering context and other properties for the scene. This constructor is protected, so it can only be called by subclasses of Scene.
    /// </summary>
    /// <param name="window">The window associated with the scene, used to determine the rendering context and other properties.</param>
    protected Scene(Window window)
    {
        this.Window = window;
    }

    ~Scene()
    {
        if (!Disposed) Console.WriteLine("Warning: Scene was not disposed before being finalized. This may cause a GL resource leak.");
    }

    /// <summary>
    /// Indicates whether the scene has been disposed and its resources released. After disposing, the scene should not be used again.
    /// </summary>
    public bool Disposed { get; private set; } = false;

    /// <summary>
    /// The list of objects contained in the scene. This list can be modified by adding or removing objects, and the Draw method will render all objects in this list.
    /// </summary>
    public List<T> Objects { get; } = new List<T>();

    /// <summary>
    /// Adds an object of type T to the scene's collection of objects. The object will be included in the scene's rendering when the Draw method is called.
    /// </summary>
    /// <param name="sceneObject">The object to add to the scene.</param>
    public void Add(T sceneObject) => Objects.Add(sceneObject);

    /// <summary>
    /// Removes an object of type T from the scene's collection of objects. If the object is not found in the collection, no action is taken. The object will no longer be included in the scene's rendering after removal.
    /// </summary>
    /// <param name="sceneObject">The object to remove from the scene.</param>
    public void Remove(T sceneObject) => Objects.Remove(sceneObject);

    /// <summary>
    /// Draws the scene using the specified shader. This method must be implemented by subclasses to define how the objects in the scene are rendered. The shader parameter provides the shader program to use for rendering, and the implementation should handle setting up any necessary matrices or state before drawing the objects.
    /// </summary>
    /// <param name="shader">The shader program to use for rendering the scene.</param>
    public abstract void Draw(Shader shader);

    /// <summary>
    /// Disposes of all objects in the scene and clears the collection. This method should be called when the scene is no longer needed to release resources held by the objects. Each object in the collection will have its Dispose method called, and then it will be removed from the collection. After calling this method, the scene's object collection will be empty.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        while (Objects.Count > 0)
        {
            Objects[0].Dispose();
            Objects.RemoveAt(0);
        }
        Disposed = true;
    }
}
