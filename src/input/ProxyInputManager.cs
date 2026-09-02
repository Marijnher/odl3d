using System;
using System.Collections.Generic;

namespace odl3d;

/// <summary>
/// A specialized InputManager for handling input events within a Scene or Object. It acts as a proxy to the Window's InputManager, allowing the owner to register and respond to key events while maintaining its own state and event subscriptions. This class is responsible for managing key press, release, and down events specific to the owner, and ensures proper disposal of event handlers when the owner is no longer active.
/// </summary>
public class ProxyInputManager : AbstractInputManager
{
    private List<Action<int>> keyPressedEvents = new List<Action<int>>();
    private List<Action<int>> keyDownEvents = new List<Action<int>>();
    private List<Action<int>> keyReleasedEvents = new List<Action<int>>();

    /// <summary>
    /// Initializes a new instance of the ProxyInputManager class for the specified window. This constructor sets up the input manager to handle key events for the owner, allowing the owner to respond to user input. It also initializes lists to keep track of registered key events for proper disposal later.
    /// </summary>
    /// <param name="window">The window associated with the owner.</param>
    public ProxyInputManager(Window window) : base(window)
    {
        Window.InputManager.OnKeyPressed += InvokeKeyPressed;
        Window.InputManager.OnKeyReleased += InvokeKeyReleased;
        Window.InputManager.OnKeyDown += InvokeKeyDown;
    }

    /// <summary>
    /// Checks if a specific key is currently being held down. This method queries the state of the key from the associated window's InputManager, allowing the owner to respond to continuous key presses during its update cycle.
    /// </summary>
    /// <param name="key">The key code of the key to check.</param>
    /// <returns>True if the key is currently being held down; otherwise, false.</returns>
    public override bool IsKeyDown(int key)
    {
        return Window.InputManager.IsKeyDown(key);
    }

    /// <summary>
    /// Registers a callback for a specific key, allowing the user to specify actions to be performed when the key is pressed, released, or held down. The provided callbacks are invoked when the corresponding key events occur, enabling custom input handling for that key within the context of the owner. This method subscribes to the window's InputManager events and maintains lists of registered events for proper disposal later.
    /// </summary>
    /// <param name="key">The key code of the key to register.</param>
    /// <param name="onPress">The action to perform when the key is pressed.</param>
    /// <param name="onRelease">The action to perform when the key is released.</param>
    /// <param name="onDown">The action to perform when the key is held down.</param>
    public override void RegisterKey(int key, Action? onPress = null, Action? onRelease = null, Action? onDown = null)
    {
        Action<int> keyPressedEvent = (pressedKey) =>
        {
            if (pressedKey == key)
                onPress?.Invoke();
        };
        Window.InputManager.OnKeyPressed += keyPressedEvent;
        keyPressedEvents.Add(keyPressedEvent);

        Action<int> keyReleasedEvent = (releasedKey) =>
        {
            if (releasedKey == key)
                onRelease?.Invoke();
        };
        Window.InputManager.OnKeyReleased += keyReleasedEvent;
        keyReleasedEvents.Add(keyReleasedEvent);

        Action<int> keyDownEvent = (downKey) =>
        {
            if (downKey == key)
                onDown?.Invoke();
        };
        Window.InputManager.OnKeyDown += keyDownEvent;
        keyDownEvents.Add(keyDownEvent);
    }

    /// <summary>
    /// Updates the input state for the owner. This method is called during the owner's update cycle to process input events and invoke the appropriate callbacks for registered keys. It allows the owner to respond to user input in real-time.
    /// </summary>
    public override void Update() { }

    /// <summary>
    /// Disposes of the ProxyInputManager and releases its resources. It unregisters all key event callbacks from the associated window's InputManager, clears the lists of registered events, and marks the ProxyInputManager as disposed. After calling this method, the ProxyInputManager should not be used again.
    /// </summary>
    public override void Dispose()
    {
        if (Disposed) return;
        keyPressedEvents.ForEach(e => Window.InputManager.OnKeyPressed -= e);
        keyPressedEvents.Clear();
        keyReleasedEvents.ForEach(e => Window.InputManager.OnKeyReleased -= e);
        keyReleasedEvents.Clear();
        keyDownEvents.ForEach(e => Window.InputManager.OnKeyDown -= e);
        keyDownEvents.Clear();
        Window.InputManager.OnKeyPressed -= InvokeKeyPressed;
        Window.InputManager.OnKeyReleased -= InvokeKeyReleased;
        Window.InputManager.OnKeyDown -= InvokeKeyDown;
        Disposed = true;
    }
}