using System;
using System.Numerics;

namespace odl3d;

public abstract class InputHost : IDisposable
{
    /// <summary>
    /// The AbstractInputManager for the object, which handles input events specific to the object. This property is null if input handling is disabled for this instance. When enabled, allows the object to register and respond to key events while maintaining its own state and event subscriptions.
    /// </summary>
    public AbstractInputManager? InputManager { get; protected set; }

    /// <summary>
    /// Indicates whether this object has been disposed and its resources released. After disposing, the object should not be used again.
    /// </summary>
    public bool Disposed { get; protected set; }

    /// <summary>
    /// Invoked when the object is disposed. Subscribers can use this event to perform cleanup or other actions when the object is no longer usable.
    /// </summary>
    public event Action? OnDisposed;

    /// <summary>
    /// Enables or disables input handling for the object. When enabled, the object will create a concrete AbstractInputManager to handle input events. When disabled, the AbstractInputManager will be disposed and input events will no longer be processed for this object. This method allows the user to control whether the object should respond to user input.
    /// </summary>
    /// <param name="enable">True to enable input handling; false to disable it.</param>
    public abstract void SetEnableInput(bool enable);

    /// <summary>
    /// Checks if a specific key is currently being held down. This method queries the state of the key from the associated input manager, allowing the object to respond to continuous key presses during its update cycle.
    /// </summary>
    /// <param name="key">The key for which to check the state.</param>
    /// <returns>True if the specified key is currently held down; otherwise, false.</returns>
    /// <exception cref="InputException">Thrown if input handling is not enabled for this object.</exception>
    public bool IsKeyDown(Key key)
    {
        if (InputManager == null) throw new InputException("Input handling is not enabled for this object. Call `SetEnableInput(true)` first.");
        return InputManager.IsKeyDown(key);
    }

    /// <summary>
    /// Checks if a specific mouse button is currently being held down. This method queries the state of the mouse button from the associated input manager, allowing the object to respond to continuous mouse input during its update cycle.
    /// </summary>
    /// <param name="button">The mouse button for which to check the state.</param>
    /// <returns>True if the specified mouse button is currently held down; otherwise, false.</returns>
    /// <exception cref="InputException">Thrown if input handling is not enabled for this object.</exception>
    public bool IsMouseDown(Mouse button)
    {
        if (InputManager == null) throw new InputException("Input handling is not enabled for this object. Call `SetEnableInput(true)` first.");
        return InputManager.IsMouseDown(button);
    }

    /// <summary>
    /// Registers a key event handler for the specified key. The onPress, onRelease, and onRepeat actions will be invoked when the corresponding key events occur. If input handling is not enabled for the object, an InputException will be thrown. This method allows the user to define custom behavior for specific keys while the object is active.
    /// </summary>
    /// <param name="key">The key for which to register the event handler.</param>
    /// <param name="onPress">The action to invoke when the key is pressed.</param>
    /// <param name="onRelease">The action to invoke when the key is released.</param>
    /// <param name="onRepeat">The action to invoke when the key is repeated.</param>
    /// <exception cref="InputException"></exception>
    public void RegisterKeyPress(Key key, Action onPress)
    {
        if (InputManager == null) throw new InputException("Input handling is not enabled for this object. Call `SetEnableInput(true)` first.");
        InputManager.RegisterKeyPress(key, onPress);
    }

    /// <summary>
    /// Registers a key down event handler for the specified key. The onDown action will be invoked when the key is pressed and held down. If input handling is not enabled for the object, an InputException will be thrown. This method allows the user to define custom behavior for specific keys while the object is active.
    /// </summary>
    /// <param name="key">The key for which to register the key down event handler.</param>
    /// <param name="onDown">The action to invoke when the key is pressed and held down.</param>
    /// <exception cref="InputException"></exception>
    public void RegisterKeyDown(Key key, Action onDown)
    {
        if (InputManager == null) throw new InputException("Input handling is not enabled for this object. Call `SetEnableInput(true)` first.");
        InputManager.RegisterKeyDown(key, onDown);
    }

    /// <summary>
    /// Registers a key repeated event handler for the specified key. The onRepeat action will be invoked when the key is repeated. If input handling is not enabled for the object, an InputException will be thrown. This method allows the user to define custom behavior for specific keys while the object is active.
    /// </summary>
    /// <param name="key">The key for which to register the key repeated event handler.</param>
    /// <param name="onRepeat">The action to invoke when the key is repeated.</param>
    /// <exception cref="InputException"></exception>
    public void RegisterKeyRepeated(Key key, Action onRepeat)
    {
        if (InputManager == null) throw new InputException("Input handling is not enabled for this object. Call `SetEnableInput(true)` first.");
        InputManager.RegisterKeyRepeated(key, onRepeat);
    }

    /// <summary>
    /// Registers a key released event handler for the specified key. The onRelease action will be invoked when the key is released. If input handling is not enabled for the object, an InputException will be thrown. This method allows the user to define custom behavior for specific keys while the object is active.
    /// </summary>
    /// <param name="key">The key for which to register the key released event handler.</param>
    /// <param name="onRelease">The action to invoke when the key is released.</param>
    /// <exception cref="InputException"></exception>
    public void RegisterKeyReleased(Key key, Action onRelease)
    {
        if (InputManager == null) throw new InputException("Input handling is not enabled for this object. Call `SetEnableInput(true)` first.");
        InputManager.RegisterKeyReleased(key, onRelease);
    }

    /// <summary>
    /// Registers a mouse press event handler for the specified mouse button. The onPress action will be invoked when the mouse button is pressed. If input handling is not enabled for the object, an InputException will be thrown. This method allows the user to define custom behavior for specific mouse buttons while the object is active.
    /// </summary>
    /// <param name="mouse">The mouse button for which to register the mouse press event handler.</param>
    /// <param name="onPress">The action to invoke when the mouse button is pressed.</param>
    /// <exception cref="InputException"></exception>
    public void RegisterMousePress(Mouse mouse, Action<Vector2> onPress)
    {
        if (InputManager == null) throw new InputException("Input handling is not enabled for this object. Call `SetEnableInput(true)` first.");
        InputManager.RegisterMousePress(mouse, onPress);
    }

    /// <summary>
    /// Registers a mouse down event handler for the specified mouse button. The onDown action will be invoked when the mouse button is pressed down. If input handling is not enabled for the object, an InputException will be thrown. This method allows the user to define custom behavior for specific mouse buttons while the object is active.
    /// </summary>
    /// <param name="mouse">The mouse button for which to register the mouse down event handler.</param>
    /// <param name="onDown">The action to invoke when the mouse button is pressed down.</param>
    /// <exception cref="InputException"></exception>
    public void RegisterMouseDown(Mouse mouse, Action<Vector2> onDown)
    {
        if (InputManager == null) throw new InputException("Input handling is not enabled for this object. Call `SetEnableInput(true)` first.");
        InputManager.RegisterMouseDown(mouse, onDown);
    }

    /// <summary>
    /// Registers a mouse repeated event handler for the specified mouse button. The onRepeat action will be invoked when the mouse button is repeatedly pressed. If input handling is not enabled for the object, an InputException will be thrown. This method allows the user to define custom behavior for specific mouse buttons while the object is active.
    /// </summary>
    /// <param name="mouse">The mouse button for which to register the mouse repeated event handler.</param>
    /// <param name="onRepeat">The action to invoke when the mouse button is repeatedly pressed.</param>
    /// <exception cref="InputException"></exception>
    public void RegisterMouseRepeated(Mouse mouse, Action<Vector2> onRepeat)
    {
        if (InputManager == null) throw new InputException("Input handling is not enabled for this object. Call `SetEnableInput(true)` first.");
        InputManager.RegisterMouseRepeated(mouse, onRepeat);
    }

    /// <summary>
    /// Registers a mouse release event handler for the specified mouse button. The onRelease action will be invoked when the mouse button is released. If input handling is not enabled for the object, an InputException will be thrown. This method allows the user to define custom behavior for specific mouse buttons while the object is active.
    /// </summary>
    /// <param name="mouse">The mouse button for which to register the mouse release event handler.</param>
    /// <param name="onRelease">The action to invoke when the mouse button is released.</param>
    /// <exception cref="InputException"></exception>
    public void RegisterMouseRelease(Mouse mouse, Action<Vector2> onRelease)
    {
        if (InputManager == null) throw new InputException("Input handling is not enabled for this object. Call `SetEnableInput(true)` first.");
        InputManager.RegisterMouseRelease(mouse, onRelease);
    }

    /// <summary>
    /// Registers a mouse moved event handler. The onMoved action will be invoked when the mouse is moved, providing the old and new mouse positions. If input handling is not enabled for the object, an InputException will be thrown. This method allows the user to define custom behavior for mouse movement while the object is active.
    /// </summary>
    /// <param name="onMoved">The action to invoke when the mouse is moved, providing the old and new mouse positions.</param>
    /// <exception cref="InputException"></exception>
    public void RegisterMouseMoved(Action<Vector2, Vector2> onMoved) 
    {
        if (InputManager == null) throw new InputException("Input handling is not enabled for this object. Call `SetEnableInput(true)` first.");
        InputManager.RegisterMouseMoved(onMoved);
    }

    /// <summary>
    /// Updates the input host by polling for input events and invoking the appropriate event handlers. This should be called once per frame to ensure that input is processed correctly.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, used to update the input host consistently with frame timing.</param>
    public virtual void Update(float deltaTime)
    {
        InputManager?.Update();
    }

    /// <summary>
    /// Disposes the input host and releases its resources. After disposing, the input host should not be used again.
    /// </summary>
    public virtual void Dispose()
    {
        if (Disposed) return;
        if (InputManager != null)
        {
            InputManager.Dispose();
            InputManager = null;
        }
        Disposed = true;
        OnDisposed?.Invoke();
    }
}