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
    public event Action<Key>? OnKeyPressed;
    
    /// <summary>
    /// An event that is triggered when a key is held down. Subscribers can register a callback to be notified when a key is continuously pressed, allowing for custom input handling in the application.
    /// </summary>
    public event Action<Key>? OnKeyDown;

    /// <summary>
    /// An event that is triggered when a key is repeated. Subscribers can register a callback to be notified when a key repeat occurs, allowing for custom input handling in the application.
    /// </summary>
    public event Action<Key>? OnKeyRepeated;
    
    /// <summary>
    /// An event that is triggered when a key is released. Subscribers can register a callback to be notified when a key release occurs, allowing for custom input handling in the application.
    /// </summary>
    public event Action<Key>? OnKeyReleased;

    /// <summary>
    /// An event that is triggered when a mouse button is pressed. Subscribers can register a callback to be notified when a mouse button press occurs, allowing for custom input handling in the application.
    /// </summary>
    public event Action<Mouse>? OnMousePressed;

    /// <summary>
    /// An event that is triggered when a mouse button is held down. Subscribers can register a callback to be notified when a mouse button is continuously pressed, allowing for custom input handling in the application.
    /// </summary>
    public event Action<Mouse>? OnMouseDown;

    /// <summary>
    /// An event that is triggered when a mouse button is repeated. Subscribers can register a callback to be notified when a mouse button repeat occurs, allowing for custom input handling in the application.
    /// </summary>
    public event Action<Mouse>? OnMouseRepeated;

    /// <summary>
    /// An event that is triggered when a mouse button is released. Subscribers can register a callback to be notified when a mouse button release occurs, allowing for custom input handling in the application.
    /// </summary>
    public event Action<Mouse>? OnMouseReleased;

    /// <summary>
    /// An event that is triggered when the mouse is moved. Subscribers can register a callback to be notified when the mouse moves, allowing for custom input handling in the application. The Vector2 parameter represents the new mouse position.
    /// </summary>
    public event Action<Vector2, Vector2>? OnMouseMoved;

    /// <summary>
    /// Creates a new InputManager for the specified window. It sets up the key callback to handle key press and release events, and initializes the keys array to track the state of each key. The InputManager will listen for input events from the window and invoke the appropriate events when keys or mouse buttons are pressed or released.
    /// </summary>
    /// <param name="window">The window for which to manage input.</param>
    public AbstractInputManager(Window window)
    {
        Window = window;
    }

    /// <summary>
    /// Registers a callback for a specific key press event.
    /// </summary>
    /// <param name="key">The key for which to register the callbacks.</param>
    /// <param name="onPress">The action to perform when the key is pressed.</param>
    public abstract void RegisterKeyPress(Key key, Action onPress);

    /// <summary>
    /// Registers a callback for a specific key down event.
    /// </summary>
    /// <param name="key">The key for which to register the callback.</param>
    /// <param name="onDown">The action to perform when the key is held down.</param>
    public abstract void RegisterKeyDown(Key key, Action onDown);

    /// <summary>
    /// Registers a callback for a specific key repeat event.
    /// </summary>
    /// <param name="key">The key for which to register the callback.</param>
    /// <param name="onRepeat">The action to perform when the key is repeated.</param>
    public abstract void RegisterKeyRepeated(Key key, Action onRepeat);

    /// <summary>
    /// Registers a callback for a specific key release event.
    /// </summary>
    /// <param name="key">The key for which to register the callback.</param>
    /// <param name="onRelease">The action to perform when the key is released.</param>
    public abstract void RegisterKeyReleased(Key key, Action onRelease);

    /// <summary>
    /// Registers a callback for a specific mouse button press event.
    /// </summary>
    /// <param name="button">The mouse button for which to register the callbacks.</param>
    /// <param name="onPress">The action to perform when the mouse button is pressed.</param>
    public abstract void RegisterMousePress(Mouse button, Action onPress);

    /// <summary>
    /// Registers a callback for a specific mouse button down event.
    /// </summary>
    /// <param name="button">The mouse button for which to register the callback.</param>
    /// <param name="onDown">The action to perform when the mouse button is held down.</param>
    public abstract void RegisterMouseDown(Mouse button, Action onDown);

    /// <summary>
    /// Registers a callback for a specific mouse button repeat event.
    /// </summary>
    /// <param name="button">The mouse button for which to register the callback.</param>
    /// <param name="onRepeat">The action to perform when the mouse button is repeated.</param>
    public abstract void RegisterMouseRepeated(Mouse button, Action onRepeat);

    /// <summary>
    /// Registers a callback for a specific mouse button release event.
    /// </summary>
    /// <param name="button">The mouse button for which to register the callback.</param>
    /// <param name="onRelease">The action to perform when the mouse button is released.</param>
    public abstract void RegisterMouseRelease(Mouse button, Action onRelease);

    /// <summary>
    /// Registers a callback for mouse movement events.
    /// </summary>
    /// <param name="onMoved">The action to perform when the mouse is moved, receiving the old and new mouse positions as Vector2.</param>
    public abstract void RegisterMouseMoved(Action<Vector2, Vector2> onMoved);

    /// <summary>
    /// Invokes the OnKeyPressed event for the specified key, notifying subscribers that the key has been pressed. This method is typically called by the input manager when a key press event is detected, allowing registered callbacks to respond to the key press.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    public void InvokeKeyPressed(Key key) => OnKeyPressed?.Invoke(key);

    /// <summary>
    /// Invokes the OnKeyDown event for the specified key, notifying subscribers that the key is currently being held down. This method is typically called by the input manager during the update cycle to allow registered callbacks to respond to continuous key presses.
    /// </summary>
    /// <param name="key">The key that is currently being held down.</param>
    public void InvokeKeyDown(Key key) => OnKeyDown?.Invoke(key);

    /// <summary>
    /// Invokes the OnKeyRepeat event for the specified key, notifying subscribers that the key is being repeated. This method is typically called by the input manager when a key repeat event is detected, allowing registered callbacks to respond to the repeated key press.
    /// </summary>
    /// <param name="key">The key that is being repeated.</param>
    public void InvokeKeyRepeated(Key key) => OnKeyRepeated?.Invoke(key);

    /// <summary>
    /// Invokes the OnKeyReleased event for the specified key, notifying subscribers that the key has been released. This method is typically called by the input manager when a key release event is detected, allowing registered callbacks to respond to the key release.
    /// </summary>
    /// <param name="key">The key that was released.</param>
    public void InvokeKeyReleased(Key key) => OnKeyReleased?.Invoke(key);

    /// <summary>
    /// Invokes the OnMousePressed event for the specified mouse button, notifying subscribers that the button has been pressed. This method is typically called by the input manager when a mouse button press event is detected, allowing registered callbacks to respond to the button press.
    /// </summary>
    /// <param name="button">The mouse button that was pressed.</param>
    public void InvokeMousePressed(Mouse button) => OnMousePressed?.Invoke(button);

    /// <summary>
    /// Invokes the OnMouseDown event for the specified mouse button, notifying subscribers that the button is currently being held down. This method is typically called by the input manager during the update cycle to allow registered callbacks to respond to continuous mouse button presses.
    /// </summary>
    /// <param name="button">The mouse button that is currently being held down.</param>
    /// 
    public void InvokeMouseDown(Mouse button) => OnMouseDown?.Invoke(button);

    /// <summary>
    /// Invokes the OnMouseRepeat event for the specified mouse button, notifying subscribers that the button is being repeated. This method is typically called by the input manager when a mouse button repeat event is detected, allowing registered callbacks to respond to the repeated button press.
    /// </summary>
    /// <param name="button">The mouse button that is being repeated.</param>
    public void InvokeMouseRepeated(Mouse button) => OnMouseRepeated?.Invoke(button);

    /// <summary>
    /// Invokes the OnMouseReleased event for the specified mouse button, notifying subscribers that the button has been released. This method is typically called by the input manager when a mouse button release event is detected, allowing registered callbacks to respond to the button release.
    /// </summary>
    /// <param name="button">The mouse button that was released.</param>
    public void InvokeMouseReleased(Mouse button) => OnMouseReleased?.Invoke(button);

    /// <summary>
    /// Invokes the OnMouseMoved event, notifying subscribers that the mouse has moved to a new position. This method is typically called by the input manager when the mouse is moved, allowing registered callbacks to respond to the mouse movement.
    /// </summary>
    /// <param name="newPosition">The new position of the mouse after it moved.</param>
    public void InvokeMouseMoved(Vector2 oldPosition, Vector2 newPosition) => OnMouseMoved?.Invoke(oldPosition, newPosition);

    /// <summary>
    /// Checks if the specified key is currently pressed. It returns true if the key is down, or false if it is up. This method can be used to query the state of keys in the Update() method or in response to input events.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is currently pressed, false otherwise.</returns>
    public abstract bool IsKeyDown(Key key);

    /// <summary>
    /// Checks if the specified mouse button is currently pressed. It returns true if the button is down, or false if it is up. This method can be used to query the state of mouse buttons in the Update() method or in response to input events.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>True if the mouse button is currently pressed, false otherwise.</returns>
    public abstract bool IsMouseDown(Mouse button);

    /// <summary>
    /// Updates the input state by checking the keys array for any keys that are currently pressed. If a key is pressed, it invokes the OnKeyDown event for that key, allowing subscribers to respond to continuous key presses. This method should be called once per frame to ensure that input events are processed and handled appropriately.
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Disposes of the InputManager and releases any resources it holds. This method should be called when the InputManager is no longer needed to prevent memory leaks and ensure proper cleanup of event subscriptions. After calling Dispose(), the InputManager should not be used again.
    /// </summary>
    public abstract void Dispose();
}