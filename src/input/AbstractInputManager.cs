using System;
using System.Collections.Generic;
using System.Numerics;

namespace odl3d;

public abstract class AbstractInputManager : IDisposable
{
    /// <summary>
    /// The window that this InputManager is associated with. It provides access to the window's properties and methods, such as its size, cursor position, and input state.
    /// </summary>
    public Window Window { get; private set; }

    /// <summary>
    /// Indicates whether the InputManager has been disposed and its resources released. After disposing, the InputManager should not be used again.
    /// </summary>
    public bool Disposed { get; protected set; } = false;

    /// <summary>
    /// An event that is triggered when a key is pressed. Subscribers can register a callback to be notified when a key press occurs, allowing for custom input handling in the application.
    /// </summary>
    public event Action<int>? OnKeyPressed;
    
    /// <summary>
    /// An event that is triggered when a key is held down. Subscribers can register a callback to be notified when a key is continuously pressed, allowing for custom input handling in the application.
    /// </summary>
    public event Action<int>? OnKeyDown;
    
    /// <summary>
    /// An event that is triggered when a key is released. Subscribers can register a callback to be notified when a key release occurs, allowing for custom input handling in the application.
    /// </summary>
    public event Action<int>? OnKeyReleased;

    /// <summary>
    /// Creates a new InputManager for the specified window. It sets up the key callback to handle key press and release events, and initializes the keys array to track the state of each key. The InputManager will listen for input events from the window and invoke the appropriate events when keys or mouse buttons are pressed or released.
    /// </summary>
    /// <param name="window">The window for which to manage input.</param>
    public AbstractInputManager(Window window)
    {
        Window = window;
    }

    /// <summary>
    /// Registers a callback for a specific key, allowing the user to specify actions to be performed when the key is pressed, released, or held down. The provided callbacks are invoked when the corresponding key events occur, enabling custom input handling for that key.
    /// </summary>
    /// <param name="key">The key for which to register the callbacks.</param>
    /// <param name="onPress">The action to perform when the key is pressed.</param>
    /// <param name="onRelease">The action to perform when the key is released.</param>
    /// <param name="onDown">The action to perform when the key is held down.</param>
    public abstract void RegisterKey(int key, Action? onPress = null, Action? onRelease = null, Action? onDown = null);

    /// <summary>
    /// Invokes the OnKeyPressed event for the specified key, notifying subscribers that the key has been pressed. This method is typically called by the input manager when a key press event is detected, allowing registered callbacks to respond to the key press.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    public void InvokeKeyPressed(int key) => OnKeyPressed?.Invoke(key);

    /// <summary>
    /// Invokes the OnKeyDown event for the specified key, notifying subscribers that the key is currently being held down. This method is typically called by the input manager during the update cycle to allow registered callbacks to respond to continuous key presses.
    /// </summary>
    /// <param name="key">The key that is currently being held down.</param>
    public void InvokeKeyDown(int key) => OnKeyDown?.Invoke(key);

    /// <summary>
    /// Invokes the OnKeyReleased event for the specified key, notifying subscribers that the key has been released. This method is typically called by the input manager when a key release event is detected, allowing registered callbacks to respond to the key release.
    /// </summary>
    /// <param name="key">The key that was released.</param>
    public void InvokeKeyReleased(int key) => OnKeyReleased?.Invoke(key);

    /// <summary>
    /// Checks if the specified key is currently pressed. It returns true if the key is down, or false if it is up. This method can be used to query the state of keys in the Update() method or in response to input events.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is currently pressed, false otherwise.</returns>
    public abstract bool IsKeyDown(int key);

    /// <summary>
    /// Updates the input state by checking the keys array for any keys that are currently pressed. If a key is pressed, it invokes the OnKeyDown event for that key, allowing subscribers to respond to continuous key presses. This method should be called once per frame to ensure that input events are processed and handled appropriately.
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Disposes of the InputManager and releases any resources it holds. This method should be called when the InputManager is no longer needed to prevent memory leaks and ensure proper cleanup of event subscriptions. After calling Dispose(), the InputManager should not be used again.
    /// </summary>
    public abstract void Dispose();
}