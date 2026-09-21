using System.Collections.Generic;
using System;
using System.Numerics;
using odl3d.Renderer;
using System.Linq;

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

    protected IRenderDevice Renderer => Window.Renderer;

    /// <summary>
    /// The camera used to render the scene. The camera's view and projection matrices are combined to create the view-projection matrix used for rendering the objects in the scene. The camera can be configured with position, orientation, field of view, aspect ratio, and other properties to control how the scene is viewed. This property is read-only for subclasses and is derived from the associated window.
    /// </summary>
    protected Camera Camera => Window.Camera;

    /// <summary>
    /// The list of objects contained in the scene. This list can be modified by adding or removing objects, and the Draw method will render all objects in this list.
    /// </summary>
    public List<T> Objects { get; } = new List<T>();

    protected ObjectShaderData[] ObjectShaderDataArray;
    protected IBuffer<ObjectShaderData> ObjectShaderDataBuffer;

    private readonly IBuffer<float> ViewProjBuffer;

    /// <summary>
    /// Initializes a new instance of the Scene class with the specified window. The window is used to determine the rendering context and other properties for the scene. This constructor is protected, so it can only be called by subclasses of Scene.
    /// </summary>
    /// <param name="window">The window associated with the scene, used to determine the rendering context and other properties.</param>
    protected unsafe Scene(Window window, int maxObjects = 100)
    {
        Window = window;
        ObjectShaderDataArray = new ObjectShaderData[maxObjects];
        ObjectShaderDataBuffer = Renderer.CreateBuffer<ObjectShaderData>(new BufferDescription
        {
            Size = maxObjects,
            Usage = BufferUsage.Uniform
        });
        ViewProjBuffer = Renderer.CreateBuffer<float>(new BufferDescription
        {
            Size = 32,
            Usage = BufferUsage.Uniform
        });
    }

    ~Scene()
    {
        if (!Disposed) Console.WriteLine("Warning: Scene was not disposed before being finalized. This may cause a renderer resource leak.");
    }

    /// <summary>
    /// Enables or disables input handling for the scene. When enabled, the scene will create a ProxyInputManager to handle input events. When disabled, the ProxyInputManager will be disposed and input events will no longer be processed for this scene. This method allows the user to control whether the scene should respond to user input.
    /// </summary>
    /// <param name="enable">True to enable input handling; false to disable it.</param>
    public override void SetEnableInput(bool enable)
    {
        if (enable && InputManager == null)
        {
            InputManager = new ProxyInputManager(Window);
        }
        else if (!enable && InputManager != null)
        {
            InputManager.Dispose();
            InputManager = null;
        }
    }

    /// <summary>
    /// Adds an object of type T to the scene's collection of objects. The object will be included in the scene's rendering when the Draw method is called.
    /// </summary>
    /// <param name="sceneObject">The object to add to the scene.</param>
    public void Add(T sceneObject) 
    {
        Objects.Add(sceneObject);
        if (Objects.Count >= 95) Console.WriteLine($"WARNING: Shader data may not support more than 100 objects.");
    }

    /// <summary>
    /// Removes an object of type T from the scene's collection of objects. If the object is not found in the collection, no action is taken. The object will no longer be included in the scene's rendering after removal.
    /// </summary>
    /// <param name="sceneObject">The object to remove from the scene.</param>
    public void Remove(T sceneObject) => Objects.Remove(sceneObject);

    public void UpdateObjectModelBuffer()
    {
        for (int i = 0; i < Objects.Count; i++)
        {
            ObjectShaderDataArray[i] = Objects[i].GetShaderData();
        }
        ObjectShaderDataBuffer.SetData(ObjectShaderDataArray, 0, Objects.Count);
    }

    protected abstract Matrix4x4 GetViewMatrix();

    protected abstract Matrix4x4 GetProjectionMatrix();

    /// <summary>
    /// Recalculates the scene's view and projection matrices and uploads them to ViewProjBuffer, then binds that buffer to shader buffer slot 1. This should be called before drawing the scene's objects so they are transformed using this scene's own view/projection rather than another scene's.
    /// </summary>
    /// <param name="pass">The render pass to bind the buffer to.</param>
    protected void UpdateViewProjBuffer(IRenderPass pass)
    {
        float[] viewProjData = new float[32];
        var view = GetViewMatrix();
        var proj = GetProjectionMatrix();
        view.ToArray().CopyTo(viewProjData, 0);
        proj.ToArray().CopyTo(viewProjData, 16);
        ViewProjBuffer.SetData(viewProjData);
        pass.SetUniformBuffer(ViewProjBuffer, 1);
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
        Objects.ForEach(o => o.Update(deltaTime));
    }

    /// <summary>
    /// Disposes of all objects in the scene and clears the collection. This method should be called when the scene is no longer needed to release resources held by the objects. Each object in the collection will have its Dispose method called, and then it will be removed from the collection. After calling this method, the scene's object collection will be empty.
    /// </summary>
    public override void Dispose()
    {
        if (Disposed) return;
        while (Objects.Count > 0)
        {
            // Child automatically removes itself from object list upon disposal
            Objects[0].Dispose();
        }
        ObjectShaderDataBuffer.Dispose();
        ViewProjBuffer.Dispose();
        base.Dispose();
        Window.RemoveScene(this);
        Disposed = true;
    }
}
