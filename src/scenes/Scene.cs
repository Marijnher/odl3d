using System.Collections.Generic;
using System;
using System.Numerics;
using odl3d.Renderer;
using System.Linq;
using System.Collections.ObjectModel;

namespace odl3d;

/// <summary>
/// A generic scene containing a collection of objects of type T, where T is constrained to be a subclass of Object. The scene provides methods to add and remove objects, and an abstract Draw method that must be implemented by subclasses to render the scene using a given shader.
/// </summary>
/// <typeparam name="T">The type of objects contained in the scene, constrained to be a subclass of Object.</typeparam>
public abstract class Scene<T> : Drawable where T : Object3D
{
    /// <summary>
    /// The window associated with the scene, used to determine the rendering context and other properties. This property is set in the constructor and is read-only for subclasses. The window provides access to the active renderer context, input handling, and other features necessary for rendering the scene's objects.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// The renderer associated with the window of the scene. This property provides access to the rendering context and other features necessary for rendering the scene's objects. It is read-only for subclasses and is derived from the associated window.
    /// </summary>
    protected IRenderDevice Renderer => Window.Renderer;

    /// <summary>
    /// The camera used to render the scene. The camera's view and projection matrices are combined to create the view-projection matrix used for rendering the objects in the scene. The camera can be configured with position, orientation, field of view, aspect ratio, and other properties to control how the scene is viewed. This property is read-only for subclasses and is derived from the associated window.
    /// </summary>
    protected Camera Camera => Window.Camera;

    /// <summary>
    /// The read-only view of objects explicitly added to the scene with Add().
    /// </summary>
    private readonly List<T> _objects = new();
    private readonly ReadOnlyCollection<T> _readOnlyObjects;
    public IReadOnlyList<T> Objects => _readOnlyObjects;

    /// <summary>
    /// The maximum number of objects that the scene can contain. This value is used to allocate the object shader data array and GPU buffer, and it can be configured in the constructor.
    /// </summary>
    protected int MaxObjects;

    /// <summary>
    /// The array of object shader data for all objects in the scene. This array is used to store the latest shader data for each object before it is uploaded to the GPU buffer.
    /// </summary>
    protected ObjectShaderData[] ObjectShaderDataArray;

    /// <summary>
    /// The GPU buffer that stores the object shader data for all objects in the scene. This buffer is updated with the latest data from the ObjectShaderDataArray before rendering.
    /// </summary>
    protected IBuffer<ObjectShaderData> ObjectShaderDataBuffer;

    /// <summary>
    /// The GPU buffer that stores the scene shader data, including the view and projection matrices. This buffer is updated with the latest scene shader data before rendering.
    /// </summary>
    protected readonly IBuffer<SceneShaderData> SceneShaderDataBuffer;

    /// <summary>
    /// Initializes a new instance of the Scene class with the specified window. The window is used to determine the rendering context and other properties for the scene. This constructor is protected, so it can only be called by subclasses of Scene.
    /// </summary>
    /// <param name="window">The window associated with the scene, used to determine the rendering context and other properties.</param>
    protected Scene(Window window, int maxObjects = 100)
    {
        Window = window;
        _readOnlyObjects = _objects.AsReadOnly();
        MaxObjects = maxObjects;
        ObjectShaderDataArray = new ObjectShaderData[maxObjects];
        ObjectShaderDataBuffer = Renderer.CreateBuffer<ObjectShaderData>(new BufferDescription
        {
            Size = maxObjects,
            Usage = BufferUsage.Uniform
        });
        SceneShaderDataBuffer = Renderer.CreateBuffer<SceneShaderData>(new BufferDescription
        {
            Size = 1,
            Usage = BufferUsage.Uniform
        });
    }

    ~Scene()
    {
        if (!Disposed) Console.WriteLine("Warning: Scene was not disposed before being finalized. This may cause a renderer resource leak.");
    }

    /// <summary>
    /// Enables input handling for this scene. If input handling is already enabled, an InvalidOperationException will be thrown.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if input handling is already enabled for this scene.</exception>
    public override void EnableInput()
    {
        if (InputManager != null)
            throw new InvalidOperationException("Input handling is already enabled for this scene.");
        InputManager = new ProxyInputManager(Window);
    }

    /// <summary>
    /// Disables input handling for this scene. If input handling is not currently enabled, an InvalidOperationException will be thrown.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if input handling is not currently enabled for this scene.</exception>
    public override void DisableInput()
    {
        if (InputManager == null)
            throw new InvalidOperationException("Input handling is not enabled for this scene.");
        InputManager.Dispose();
        InputManager = null;
    }

    /// <summary>
    /// Adds an object of type T to the scene's collection of objects. The object will be included in the scene's rendering when the Draw method is called.
    /// </summary>
    /// <param name="sceneObject">The object to add to the scene.</param>
    public void Add(T sceneObject) 
    {
        ArgumentNullException.ThrowIfNull(sceneObject);
        if (sceneObject.Scene != null)
            throw new InvalidOperationException("The object already belongs to a scene. Remove it before adding it elsewhere.");
        if (_objects.Count >= MaxObjects)
            throw new RenderException($"Cannot add more than {MaxObjects} objects to the scene.");
        if (this is not Scene<Object3D> objectScene)
            throw new InvalidOperationException("Only scenes containing Object3D instances can accept drawable objects.");
        sceneObject.Attach(objectScene);
        _objects.Add(sceneObject);
    }

    /// <summary>
    /// Removes an object of type T from the scene's collection of objects. If the object is not found in the collection, no action is taken. The object will no longer be included in the scene's rendering after removal.
    /// </summary>
    /// <param name="sceneObject">The object to remove from the scene.</param>
    public void Remove(T sceneObject)
    {
        if (_objects.Remove(sceneObject))
            sceneObject.Detach(this as Scene<Object3D>);
    }

    /// <summary>
    /// Binds the object shader data to the GPU buffer, updating it with the latest data from all objects in the scene.
    /// </summary>
    internal void UpdateObjectShaderData()
    {
        for (int i = 0; i < _objects.Count; i++)
        {
            ObjectShaderDataArray[i] = _objects[i].GetShaderData();
        }
        ObjectShaderDataBuffer.SetData(ObjectShaderDataArray, 0, _objects.Count);
    }

    /// <summary>
    /// Gets the view matrix for the scene, which defines the camera's position and orientation in the 3D world.
    /// </summary>
    /// <returns>The view matrix for the scene.</returns>
    protected abstract Matrix4x4 GetViewMatrix();

    /// <summary>
    /// Gets the projection matrix for the scene, which defines how 3D points are projected onto the 2D screen.
    /// </summary>
    /// <returns>The projection matrix for the scene.</returns>
    protected abstract Matrix4x4 GetProjectionMatrix();

    /// <summary>
    /// Recalculates the scene's view and projection matrices and uploads them to ViewProjBuffer, then binds that buffer to shader buffer slot 1. This should be called before drawing the scene's objects so they are transformed using this scene's own view/projection rather than another scene's.
    /// </summary>
    /// <param name="pass">The render pass to bind the buffer to.</param>
    protected void UpdateSceneShaderData(IRenderPass pass)
    {
        SceneShaderDataBuffer.SetData([new SceneShaderData
        {
            Projection = GetProjectionMatrix(),
            View = GetViewMatrix()
        }]);
        pass.SetUniformBuffer(SceneShaderDataBuffer, 1);
    }

    /// <summary>
    /// Draws the scene using the specified shader. This method must be implemented by subclasses to define how the objects in the scene are rendered. The shader parameter provides the shader program to use for rendering, and the implementation should handle setting up any necessary matrices or state before drawing the objects.
    /// </summary>
    /// <param name="shader">The shader program to use for rendering the scene.</param>
    /// <param name="renderPass">The render pass to use for rendering the scene.</param>
    public abstract void Draw(IRenderPass pass, RenderPass passType = RenderPass.Opaque);

    /// <summary>
    /// Updates all objects in the scene by calling their Update methods. This should be called once per frame to ensure that the scene and its objects are updated correctly.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, used to update the scene's objects consistently with frame timing.</param>
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        foreach (T sceneObject in _objects)
            sceneObject.Update(deltaTime);
    }

    /// <summary>
    /// Disposes of all objects in the scene and clears the collection. This method should be called when the scene is no longer needed to release resources held by the objects. Each object in the collection will have its Dispose method called, and then it will be removed from the collection. After calling this method, the scene's object collection will be empty.
    /// </summary>
    public override void Dispose()
    {
        if (Disposed) return;
        while (_objects.Count > 0)
        {
            // Child automatically removes itself from object list upon disposal
            _objects[0].Dispose();
        }
        ObjectShaderDataBuffer.Dispose();
        SceneShaderDataBuffer.Dispose();
        base.Dispose();
        Window.RemoveScene(this);
        Disposed = true;
    }
}
